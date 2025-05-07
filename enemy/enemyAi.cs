using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyAi : MonoBehaviour
{
    // Enum to track AI state
    private enum AIState { Idle, Chasing, CatchingPlayer }
    private AIState currentState = AIState.Idle;
    
    [Header("Target")]
    [Tooltip("The player transform to follow. Will auto-find if not set.")]
    public Transform player;
    
    [Tooltip("Vertical offset to target the player's head instead of center")]
    public float targetHeadOffset = 1.7f;

    [Header("Follow Settings")]
    [Tooltip("Maximum distance to detect and follow the player")]
    public float maxFollowRange = 15f;
    
    [Tooltip("Minimum distance to keep from the player")]
    public float minFollowDistance = 2f;
    
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
    
    [Header("Roaming Settings")]
    [Tooltip("Movement speed when roaming")]
    public float roamSpeed = 0.7f;
    
    [Tooltip("Maximum distance to wander from spawn point")]
    public float roamRadius = 10f;
    
    [Tooltip("How long to wait at each point before moving to the next")]
    public float waitTimeAtPoint = 2f;

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

    private Vector3 spawnPosition;
    private Vector3 targetRoamPosition;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    private float outOfSightTimer = 0f;
    private bool isOutOfSightTimerActive = false;
    
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
                    Debug.LogWarning("enemyAi could not find player. Please assign manually.");
                }
            }
        }
        
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
            animator.SetBool("isPlayerInsight", false);
        }
        
        // Store the initial position as the spawn point
        spawnPosition = transform.position;
        
        // Set initial roam position
        SetNewRoamTarget();

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
                   
        // Check if player is visible
        CheckPlayerVisibility();
        
        // Handle out of sight timer
        UpdateOutOfSightTimer();
        
        // Handle state-based behavior
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
                
            case AIState.Chasing:
                HandleChasingState();
                break;
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
            {
                if (animator != null)
                {
                    animator.SetBool("isPlayerInsight", false);
                    
                    // If in chase state, set the speed to outOfSightSpeed
                    if (currentState == AIState.Chasing)
                    {
                        animator.SetFloat("speed", outOfSightSpeed);
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
            // Add memory/persistence (could be enhanced with a timer)
            if (distanceToPlayer > maxFollowRange * 1.5f) // Give extra range before giving up
            {
                currentState = AIState.Idle;
                SetNewRoamTarget();
            }
        }
    }
    
    private void HandleChasingState()
    {
        if (player == null)
            return;
        
        // Calculate actual chase speed based on whether player is in sight
        float currentChaseSpeed = isPlayerInSight ? chaseSpeed * inSightSpeedMultiplier : outOfSightSpeed;
                   
        // Pass actual movement speed to animator instead of boolean flags
        if (animator != null)
        {
            animator.SetFloat("speed", currentChaseSpeed);
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
        // Update animation state based on actual movement speed
        if (animator != null)
        {
            float currentSpeed = isWaiting ? 0 : roamSpeed;
            animator.SetFloat("speed", currentSpeed);
        }
        
        if (isWaiting)
        {
            // Wait at current position
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                SetNewRoamTarget();
            }
            
            // Make sure the enemy stays still when waiting
            if (rb != null)
            {
                Vector3 currentVelocity = rb.velocity;
                rb.velocity = new Vector3(0, currentVelocity.y, 0);
            }
        }
        else
        {
            // Move toward roam target
            Vector3 direction = targetRoamPosition - transform.position;
            direction.y = 0;
            
            if (direction != Vector3.zero)
            {
                // Rotate toward target
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed * 0.5f);
                
                // Calculate normalized direction and distance
                Vector3 moveDirection = direction.normalized;
                float moveDistance = roamSpeed * Time.deltaTime;
                
                // Apply movement using Rigidbody if available
                if (rb != null)
                {
                    // Keep the y velocity (gravity) and only modify xz movement
                    rb.MovePosition(rb.position + moveDirection * moveDistance);
                }
                else
                {
                    // Fallback to transform if no Rigidbody
                    Vector3 newPosition = Vector3.MoveTowards(
                        transform.position,
                        new Vector3(targetRoamPosition.x, transform.position.y, targetRoamPosition.z),
                        moveDistance);
                    
                    transform.position = new Vector3(newPosition.x, transform.position.y, newPosition.z);
                    AlignToGround();
                }
            }
            
            // Check if we've reached the target (horizontal distance only)
            float horizontalDistance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.z), 
                new Vector2(targetRoamPosition.x, targetRoamPosition.z));
                
            if (horizontalDistance < 0.5f)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
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
        }
    }
    
    // Make this method public so it can be called from checkSafe
    public void SetNewRoamTarget()
    {
        // Get random point within roam radius of spawn point
        Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
        Vector3 randomPoint = spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
        
        // Check for valid ground position
        RaycastHit hit;
        if (Physics.Raycast(randomPoint + Vector3.up * 10, Vector3.down, out hit, 20f))
        {
            // Use the hit point as the target position
            randomPoint = hit.point;
        }
        
        // Check if point is in a safe zone, if so, find an alternative
        if (checkSafe.IsPositionInSafeZone(randomPoint))
        {
            // Try to find a safe position
            randomPoint = checkSafe.GetNearestSafePosition(randomPoint, transform.position);
        }
        
        targetRoamPosition = randomPoint;
    }

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
                animator.SetBool("isPlayerInsight", true);
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

    // Keep this CancelChase method in enemyAi for internal state management
    public void CancelChase()
    {
        // Reset to idle state when entering a safe zone
        currentState = AIState.Idle;
        
        // Reset animation state
        if (animator != null)
        {
            animator.SetBool("isPlayerInsight", false);
            animator.SetFloat("speed", 0);
        }
        
        // Set a new roam target away from the safe zone
        Vector3 safePosition = checkSafe.GetNearestPointOutsideSafeZone(transform.position);
        targetRoamPosition = safePosition;
        isWaiting = false;
        
        Debug.Log("Enemy chase canceled due to safe zone");
    }

    // Remove or comment out these methods since they are now in checkSafe.cs
    // public void TeleportToSpawnPosition() { ... }
    // private IEnumerator DisappearAndTeleport() { ... }

    // Visualize the follow range and vision in the editor
    private void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // Draw max follow range
        Gizmos.color = isPlayerInRange ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxFollowRange);
        
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
        
        // Draw roam radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(spawnPosition, roamRadius);
        
        // Draw current target position
        if (currentState == AIState.Idle)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(targetRoamPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetRoamPosition);
        }
    }
}
