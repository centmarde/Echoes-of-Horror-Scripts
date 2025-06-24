using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAi2 : MonoBehaviour
{
    // Enum to track AI state
    private enum AIState { Idle, Chasing, CatchingPlayer, Frozen }
    private AIState currentState = AIState.Idle;
    
    [Header("Target")]
    [Tooltip("The player transform to follow. Will auto-find if not set.")]
    public Transform player;
    
    [Tooltip("Vertical offset to target the player's head instead of center")]
    public float targetHeadOffset = 1.7f;

    [Header("Follow Settings")]
    [Tooltip("Maximum distance to detect and follow the player")]
    public float maxFollowRange = 40f;
    
    [Tooltip("Minimum distance to keep from the player")]
    public float minFollowDistance = 2f;
    
    [Tooltip("Proximity range where enemy can sense player regardless of vision")]
    public float proximityDetectionRange = 45f;
    
    [Tooltip("Movement speed when the player is out of detection radius")]
    public float slowSpeed = 0.3f;
    
    [Tooltip("Movement speed when following player at normal pace")]
    public float normalSpeed = 1.0f;
    
    [Tooltip("Movement speed when actively chasing the player")]
    public float chaseSpeed = 2f;
    
    [Tooltip("Speed when player is not in direct sight")]
    public float outOfSightSpeed = 0.3f;
    
    [Tooltip("Speed multiplier when player is in direct sight")]
    public float inSightSpeedMultiplier = 3f;
    
    [Tooltip("How fast the monster rotates to face the player")]
    public float rotationSpeed = 3f;
    
    [Header("Vision Settings")]
    [Tooltip("Field of view angle in degrees")]
    [Range(0, 360)]
    public float visionAngle = 90f;
    
    [Tooltip("Can the enemy see through obstacles?")]
    public bool requireLineOfSight = false;
    
    [Tooltip("Layer mask for vision obstruction detection")]
    public LayerMask visionObstructionMask;
      [Header("Player Watching Detection")]
    [Tooltip("Reference to the player watching detector component")]
    public PlayerWatchingDetector watchingDetector;
    
    [Tooltip("How long to stay frozen when player looks away")]
    public float freezeGracePeriod = 0.1f;
    
    [Tooltip("Should the enemy completely stop or just slow down when watched?")]
    public bool completelyStopWhenWatched = true;
    
    [Tooltip("Speed multiplier when being watched (if not completely stopped)")]
    [Range(0f, 1f)]
    public float watchedSpeedMultiplier = 0.1f;

    [Header("Animation")]
    [Tooltip("Reference to the Animator component")]
    public Animator animator;
    
    [Header("Ground Detection")]
    [Tooltip("Height offset from ground")]
    public float groundOffset = 0.1f;
    [Tooltip("Maximum distance to check for ground")]
    public float groundCheckDistance = 10f;
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool isPlayerInRange = false;
    [SerializeField] private bool isPlayerInSight = false;
    [SerializeField] private bool isCatchSequenceActive = false;
    [SerializeField] private bool isBeingWatched = false;
    [SerializeField] private bool isFrozen = false;    private Vector3 spawnPosition;
    private float outOfSightTimer = 0f;
    private bool isOutOfSightTimerActive = false;
    private float freezeTimer = 0f;
    
    // Add Rigidbody reference
    private Rigidbody rb;

    // Add reference to the checkSafe component
    private checkSafe safeZoneChecker;

    private void Start()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        // If we have a Rigidbody, configure it properly
        if (rb != null)
        {
            rb.freezeRotation = true; // Prevent physics from rotating the enemy
            rb.useGravity = true; // Use gravity to keep it grounded
        }
        
        // If player is not manually assigned, try to find the player using multiple methods
        if (player == null)
        {
            // First try to find by tag if the player has "Player" tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                // Fallback to finding by component type
                FirstPersonController fpc = (FirstPersonController)FindObjectOfType(typeof(FirstPersonController));
                if (fpc != null)
                {
                    player = fpc.transform;
                }
                else
                {
                    Debug.LogWarning("enemyAi2 could not find player. Please assign manually.");
                }
            }
        }
          // Note: PlayerWatchingDetector is now optional since we have built-in face detection
        // You can still manually assign it if you want to use the advanced detector features
        
        // Get the animator component if not manually assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("No Animator component found on enemy. Animation functionality will be disabled.");
            }
        }
          // Initialize animation parameters
        if (animator != null)
        {
            SetAnimatorBoolSafe("isPlayerInsight", false);
            SetAnimatorBoolSafe("isFrozen", false);
        }
          // Store the initial position as the spawn point
        spawnPosition = transform.position;

        // Get or add the checkSafe component
        safeZoneChecker = GetComponent<checkSafe>();
        if (safeZoneChecker == null)
        {
            safeZoneChecker = gameObject.AddComponent<checkSafe>();
        }
    }

    private void Update()
    {
        if (player == null)
            return;
        
        // Skip all normal AI behavior if in catch sequence
        if (isCatchSequenceActive)
            return;
        
        // Check if player is watching us
        CheckIfBeingWatched();
        
        // Handle freeze state when being watched
        HandleWatchingBehavior();
                   
        // Check if player is visible (only if not frozen)
        if (!isFrozen)
        {
            CheckPlayerVisibility();
        }
        
        // Handle out of sight timer
        UpdateOutOfSightTimer();
        
        // Handle state-based behavior
        switch (currentState)
        {
            case AIState.Idle:
                if (!isFrozen) HandleIdleState();
                break;
                
            case AIState.Chasing:
                if (!isFrozen) HandleChasingState();
                break;
                
            case AIState.Frozen:
                HandleFrozenState();
                break;
        }
    }
      private void CheckIfBeingWatched()
    {
        // Always use manual check for instant face detection
        if (player != null)
        {
            Camera playerCamera = player.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                Vector3 directionToEnemy = (transform.position - playerCamera.transform.position).normalized;
                float dotProduct = Vector3.Dot(playerCamera.transform.forward, directionToEnemy);
                
                // Check if player is facing our direction (more lenient threshold for instant detection)
                float lookingThreshold = 0.3f; // Lower threshold = wider detection angle
                bool isFacingEnemy = dotProduct > lookingThreshold;
                
                // Check distance - only freeze if close enough
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                bool isInRange = distanceToPlayer <= maxFollowRange;
                
                // Check line of sight if required
                bool hasLineOfSight = true;
                if (requireLineOfSight && isFacingEnemy && isInRange)
                {
                    RaycastHit hit;
                    Vector3 rayStart = playerCamera.transform.position;
                    Vector3 rayDirection = directionToEnemy;
                    
                    if (Physics.Raycast(rayStart, rayDirection, out hit, distanceToPlayer, visionObstructionMask))
                    {
                        // If we hit something other than this enemy, line of sight is blocked
                        if (hit.transform.gameObject != this.gameObject)
                        {
                            hasLineOfSight = false;
                        }
                    }
                }
                
                // Set watching state based on all conditions
                isBeingWatched = isFacingEnemy && isInRange && hasLineOfSight;
                
                if (showDebugInfo && isBeingWatched)
                {
                    Debug.Log($"Player is facing {gameObject.name} - FREEZING!");
                }
            }
        }
    }
    
    private void HandleWatchingBehavior()
    {
        if (isBeingWatched)
        {
            // Player is watching us - freeze or slow down
            if (completelyStopWhenWatched)
            {
                if (currentState != AIState.Frozen)
                {
                    currentState = AIState.Frozen;
                    isFrozen = true;
                    freezeTimer = 0f;
                    
                    if (showDebugInfo)
                    {
                        Debug.Log("Enemy frozen - player is watching!");
                    }
                }
            }
            
            // Reset freeze timer while being watched
            freezeTimer = 0f;
        }
        else if (isFrozen)
        {
            // Player stopped watching - start grace period timer
            freezeTimer += Time.deltaTime;
            
            if (freezeTimer >= freezeGracePeriod)
            {
                // Grace period over - unfreeze
                isFrozen = false;
                currentState = AIState.Idle; // Return to normal behavior
                
                if (showDebugInfo)
                {
                    Debug.Log("Enemy unfrozen - grace period ended");
                }
            }
        }
    }
      private void HandleFrozenState()
    {
        // Stop all movement when frozen
        if (rb != null)
        {
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(0, currentVelocity.y, 0);
        }
          // Update animation to show frozen state
        if (animator != null)
        {
            SetAnimatorFloatSafe("speed", 0);
            SetAnimatorBoolSafe("isFrozen", true);
        }
    }
    
    private void UpdateOutOfSightTimer()
    {
        // If player just went out of sight, start the timer
        if (!isPlayerInSight && isOutOfSightTimerActive)
        {
            outOfSightTimer += Time.deltaTime;
            
            // After 1 second, stop the animation
            if (outOfSightTimer >= 1.0f)
            {                if (animator != null)
                {
                    SetAnimatorBoolSafe("isPlayerInsight", false);
                    
                    // If in chase state, set the speed to outOfSightSpeed
                    if (currentState == AIState.Chasing)
                    {
                        float effectiveSpeed = isBeingWatched && !completelyStopWhenWatched ? 
                            outOfSightSpeed * watchedSpeedMultiplier : outOfSightSpeed;
                        SetAnimatorFloatSafe("speed", effectiveSpeed);
                    }
                }
                
                isOutOfSightTimerActive = false;
                outOfSightTimer = 0f;
            }
        }
    }
    
    private void CheckPlayerVisibility()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        isPlayerInRange = distanceToPlayer <= maxFollowRange;
        
        // First check proximity - can sense player regardless of vision cone if very close
        if (distanceToPlayer <= proximityDetectionRange)
        {
            // Check if something is blocking direct line to player
            Vector3 playerHeadPosition = player.position + new Vector3(0, targetHeadOffset, 0);
            Vector3 directionToPlayer = (playerHeadPosition - transform.position).normalized;
            
            bool hasLineOfSight = true;
            if (requireLineOfSight)
            {
                // Start raycast from enemy's eye level
                Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
                
                // Cast ray to player's head
                RaycastHit hit;
                if (Physics.Raycast(eyePosition, directionToPlayer, out hit, proximityDetectionRange, visionObstructionMask))
                {
                    // If we hit something that's not the player, we don't have line of sight
                    if (hit.transform != player)
                    {
                        hasLineOfSight = false;
                    }
                }
            }
            
            if (hasLineOfSight || !requireLineOfSight)
            {
                // Player is very close! Alert the enemy regardless of vision cone
                if (showDebugInfo && !isPlayerInSight)
                {
                    Debug.Log("Enemy detected player by proximity!");
                }
                
                // If player was previously not in sight, reset the timer
                if (!isPlayerInSight)
                {
                    isOutOfSightTimerActive = false;
                    outOfSightTimer = 0f;
                }
                
                isPlayerInSight = true;
                // Update animation parameter immediately
                if (animator != null)
                {
                    animator.SetBool("isPlayerInsight", true);
                    animator.SetBool("isFrozen", false);
                }
                currentState = AIState.Chasing;
                return;
            }
        }
           
        if (isPlayerInRange)
        {
            // Calculate direction to player's head instead of center
            Vector3 playerHeadPosition = player.position + new Vector3(0, targetHeadOffset, 0);
            Vector3 directionToPlayer = (playerHeadPosition - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer <= visionAngle / 2)
            {
                // Check for line of sight if required - aim for head position
                bool hasLineOfSight = true;
                
                if (requireLineOfSight)
                {
                    // Start raycast from enemy's eye level
                    Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
                    
                    // Cast ray to player's head
                    RaycastHit hit;
                    if (Physics.Raycast(eyePosition, directionToPlayer, out hit, maxFollowRange, visionObstructionMask))
                    {
                        // If we hit something that's not the player, we don't have line of sight
                        if (hit.transform != player)
                        {
                            hasLineOfSight = false;
                        }
                    }
                }
                
                if (hasLineOfSight)
                {
                    // If player was previously not in sight, reset the timer
                    if (!isPlayerInSight)
                    {
                        isOutOfSightTimerActive = false;
                        outOfSightTimer = 0f;
                    }
                    
                    isPlayerInSight = true;
                    // Update animation parameter immediately
                    if (animator != null)
                    {
                        animator.SetBool("isPlayerInsight", true);
                        animator.SetBool("isFrozen", false);
                    }
                    currentState = AIState.Chasing;
                    return;
                }
            }
        }
        
        // If we reach here, player is not in sight
        bool wasInSight = isPlayerInSight;
        isPlayerInSight = false;
        
        // If player just went out of sight, start the timer
        if (wasInSight)
        {
            isOutOfSightTimerActive = true;
            outOfSightTimer = 0f;
            
            // Don't immediately change the animator parameter here
            // Let the UpdateOutOfSightTimer handle it after delay
        }
        
        // If we were chasing but lost sight of player, wait a bit before going back to idle
        if (currentState == AIState.Chasing && !isPlayerInSight)
        {
            // Add memory/persistence (could be enhanced with a timer)            if (distanceToPlayer > maxFollowRange * 1.5f) // Give extra range before giving up
            {
                currentState = AIState.Idle;
            }
        }
    }
    
    private void HandleChasingState()
    {
        if (player == null)
            return;
        
        // Calculate actual chase speed based on whether player is in sight and being watched
        float currentChaseSpeed = isPlayerInSight ? chaseSpeed * inSightSpeedMultiplier : outOfSightSpeed;
        
        // Apply watching speed modifier if being watched but not completely stopped
        if (isBeingWatched && !completelyStopWhenWatched)
        {
            currentChaseSpeed *= watchedSpeedMultiplier;
        }
                   
        // Pass actual movement speed to animator instead of boolean flags
        if (animator != null)
        {
            animator.SetFloat("speed", currentChaseSpeed);
            animator.SetBool("isFrozen", false);
        }
            
        // Calculate direction to player's head position instead of center
        Vector3 playerHeadPosition = player.position + new Vector3(0, targetHeadOffset, 0);
        Vector3 direction = playerHeadPosition - transform.position;
        Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z);
        
        // Check if the player is more than 90 degrees from forward to prevent backward movement
        float angleToPlayer = Vector3.Angle(transform.forward, horizontalDirection);
        bool isFacingPlayer = angleToPlayer < 90f;
            
        if (horizontalDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(horizontalDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
        
        // Move towards player if further than minimum distance
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > minFollowDistance && isFacingPlayer)
        {
            // Calculate target position (only x and z from player)
            Vector3 targetPosition = new Vector3(
                player.position.x, 
                transform.position.y, 
                player.position.z);
            
            // Check if target is in a safe zone and get safe position if needed
            if (checkSafe.IsPositionInSafeZone(targetPosition) || 
                checkSafe.IsPathCrossingSafeZone(transform.position, targetPosition))
            {
                // Get a safe position to move to instead
                targetPosition = checkSafe.GetNearestSafePosition(targetPosition, transform.position);
                
                // If we're already in a safe zone, prioritize getting out
                if (checkSafe.IsPositionInSafeZone(transform.position))
                {
                    targetPosition = checkSafe.GetNearestPointOutsideSafeZone(transform.position);
                }
            }
            
            // Use NavMesh or physics-based movement to respect colliders
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            
            // Raycast to check for obstacles
            RaycastHit hit;
            float moveDistance = currentChaseSpeed * Time.deltaTime;
            bool hitObstacle = Physics.Raycast(transform.position + Vector3.up * 0.1f, moveDirection, out hit, moveDistance + 0.5f);
            
            if (hitObstacle)
            {
                // Try to find an alternate path around the obstacle
                for (int i = 15; i <= 165; i += 15)
                {
                    // Try rotating left and right to find a clear path
                    Vector3 leftDir = Quaternion.Euler(0, -i, 0) * moveDirection;
                    Vector3 rightDir = Quaternion.Euler(0, i, 0) * moveDirection;
                    
                    if (!Physics.Raycast(transform.position + Vector3.up * 0.1f, leftDir, moveDistance + 0.5f))
                    {
                        moveDirection = leftDir;
                        break;
                    }
                    else if (!Physics.Raycast(transform.position + Vector3.up * 0.1f, rightDir, moveDistance + 0.5f))
                    {
                        moveDirection = rightDir;
                        break;
                    }
                }
            }
            
            // Apply movement using Rigidbody if available, or transform if not
            if (rb != null)
            {
                // Keep the y velocity as is (affected by gravity) and only modify xz movement
                Vector3 currentVelocity = rb.velocity;
                Vector3 targetVelocity = new Vector3(moveDirection.x * currentChaseSpeed, currentVelocity.y, moveDirection.z * currentChaseSpeed);
                
                // Option 1: Use MovePosition (better for physics interactions)
                rb.MovePosition(rb.position + moveDirection * moveDistance);
                
                // Option 2: Set velocity directly (alternative method)
                // rb.velocity = targetVelocity;
            }
            else
            {
                // Fallback to using transform directly if no Rigidbody
                Vector3 movePosition = transform.position + moveDirection * moveDistance;
                transform.position = new Vector3(movePosition.x, transform.position.y, movePosition.z);
                AlignToGround();
            }
        }
        else if (rb != null)
        {
            // Stop horizontal movement when close to player
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(0, currentVelocity.y, 0);
        }
    }
      private void HandleIdleState()
    {
        // In idle state, enemy just stands still and watches for the player
        if (animator != null)
        {
            SetAnimatorFloatSafe("speed", 0);
            SetAnimatorBoolSafe("isFrozen", false);
        }
        
        // Stop all movement when idle
        if (rb != null)
        {
            Vector3 currentVelocity = rb.velocity;
            rb.velocity = new Vector3(0, currentVelocity.y, 0);
        }
    }
    
    // This method is now only used as a fallback when Rigidbody is not available
    private void AlignToGround()
    {
        if (rb != null) return; // Skip if using Rigidbody
        
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * groundCheckDistance * 0.5f;
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            // Set position to ground height plus offset
            transform.position = new Vector3(
                transform.position.x,
                hit.point.y + groundOffset,
                transform.position.z
            );
        }    }

    // New method to handle catch sequence state
    public void SetCatchSequenceActive(bool active)
    {
        isCatchSequenceActive = active;
        
        if (active)
        {
            // Change state to catching player
            currentState = AIState.CatchingPlayer;
              // Ensure animation parameter stays true during catch sequence
            if (animator != null)
            {
                SetAnimatorBoolSafe("isPlayerInsight", true);
                SetAnimatorBoolSafe("isFrozen", false);
            }
        }
        else
        {
            // Return to normal AI behavior
            currentState = AIState.Idle;
            
            // Reset animation state if needed
            if (animator != null)
            {
                // Check visibility again to set the correct state
                CheckPlayerVisibility();
            }
        }
    }

    // Add a ResetState method to handle resetting the enemy state after a catch sequence
    public void ResetState()
    {
        // Reset to idle state
        currentState = AIState.Idle;
        
        // Reset animation parameters
        if (animator != null)
        {
            animator.SetBool("isPlayerInsight", false);
            animator.SetBool("isFrozen", false);
            animator.SetFloat("speed", 0);
        }
          // Reset pursuit variables
        isOutOfSightTimerActive = false;
        outOfSightTimer = 0f;
        isFrozen = false;
        freezeTimer = 0f;
        
        // Reset catch sequence flag
        isCatchSequenceActive = false;
        
        // Re-enable the AI component just in case
        enabled = true;
    }    // Keep this CancelChase method in enemyAi for internal state management
    public void CancelChase()
    {
        // Reset to idle state when entering a safe zone
        currentState = AIState.Idle;
        
        // Reset animation state
        if (animator != null)
        {
            SetAnimatorBoolSafe("isPlayerInsight", false);
            SetAnimatorBoolSafe("isFrozen", false);
            SetAnimatorFloatSafe("speed", 0);
        }
        
        Debug.Log("Enemy chase canceled due to safe zone");
    }

    // Public method to check if enemy is being watched (for external components)
    public bool IsBeingWatched()
    {
        return isBeingWatched;
    }

    // Public method to force freeze/unfreeze (for external components)
    public void ForceFreeze(bool freeze)
    {
        if (freeze)
        {
            currentState = AIState.Frozen;
            isFrozen = true;
        }
        else
        {
            isFrozen = false;
            currentState = AIState.Idle;
        }
    }

    // Helper method to safely set animator bool parameters
    private void SetAnimatorBoolSafe(string parameterName, bool value)
    {
        if (animator != null)
        {
            // Check if the parameter exists before trying to set it
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameterName && param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(parameterName, value);
                    return;
                }
            }
            
            // Parameter doesn't exist - log a warning only once per parameter if debug is enabled
            if (showDebugInfo)
            {
                Debug.LogWarning($"Animator parameter '{parameterName}' not found on {gameObject.name}. This is optional and can be ignored if you don't need this animation state.");
            }
        }
    }
    
    // Helper method to safely set animator float parameters
    private void SetAnimatorFloatSafe(string parameterName, float value)
    {
        if (animator != null)
        {
            // Check if the parameter exists before trying to set it
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == parameterName && param.type == AnimatorControllerParameterType.Float)
                {
                    animator.SetFloat(parameterName, value);
                    return;
                }
            }
            
            // Parameter doesn't exist - this is fine, just skip it silently
            if (showDebugInfo)
            {
                Debug.LogWarning($"Animator parameter '{parameterName}' not found on {gameObject.name}. This is optional and can be ignored if you don't need this animation state.");
            }
        }
    }

    // Visualize the follow range and vision in the editor
    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // Draw max follow range
        Gizmos.color = isPlayerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxFollowRange);
        
        // Draw proximity detection range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f); // Orange
        Gizmos.DrawWireSphere(transform.position, proximityDetectionRange);
        
        // Draw min follow distance
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minFollowDistance);
        
        // Draw vision cone
        Gizmos.color = isPlayerInSight ? Color.green : Color.gray;
        Vector3 leftRayDirection = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * maxFollowRange);
        Gizmos.DrawRay(transform.position, rightRayDirection * maxFollowRange);
          // Draw line of sight ray if player is in range
        if (player != null && requireLineOfSight && Vector3.Distance(transform.position, player.position) <= maxFollowRange)
        {
            Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
            Vector3 playerHeadPosition = player.position + new Vector3(0, targetHeadOffset, 0);
            Vector3 directionToPlayer = (playerHeadPosition - eyePosition).normalized;
            
            if (isPlayerInSight)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;
                
            Gizmos.DrawLine(eyePosition, eyePosition + directionToPlayer * maxFollowRange);
        }
        
        // Draw freeze indicator
        if (isFrozen || isBeingWatched)
        {
            Gizmos.color = isBeingWatched ? Color.red : Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }
    }
}
