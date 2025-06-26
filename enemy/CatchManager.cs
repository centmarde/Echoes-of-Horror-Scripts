using System.Collections;
using UnityEngine;

public class CatchManager : MonoBehaviour
{
    [Header("Catch Settings")]
    [Tooltip("The enemy AI component")]
    public enemyAi2 enemyAI;
    
    [Tooltip("The player's FirstPersonController component")]
    public FirstPersonController playerController;
    
    [Tooltip("Minimum distance to player before catch can occur")]
    public float catchRange = 2.5f;
    
    [Tooltip("Should the player respawn after being caught?")]
    public bool respawnPlayerOnCatch = true;
    
    [Tooltip("Respawn delay after catch")]
    public float respawnDelay = 2f;
    
    [Tooltip("Transform to use as the respawn position (optional)")]
    public Transform respawnPoint;
    
    [Header("Monster Settings")]
    [Tooltip("How fast the monster turns to face the player")]
    public float monsterTurnSpeed = 10f;
    
    [Header("Camera Settings")]
    [Tooltip("How fast the player camera turns to face the monster")]
    public float cameraRotationSpeed = 5f;
    
    [Tooltip("Duration of camera shake")]
    public float shakeDuration = 1.5f;
    
    [Tooltip("Amplitude of camera shake")]
    public float shakeAmount = 0.1f;
    
    [Tooltip("Y-axis offset for where the camera looks at the monster (positive = higher, negative = lower)")]
    public float monsterFaceYOffset = 3f;
    
    [Header("Player Lifting")]
    [Tooltip("How much to lift the player during catch sequence")]
    public float playerLiftAmount = 1f;
    
    [Tooltip("How fast to lift the player")]
    public float playerLiftSpeed = 2f;
    
    [Tooltip("Duration to lift the player during catch sequence")]
    public float playerLiftDuration = 5f;
    
    [Header("Monster Player Distance")]
    [Tooltip("Distance between player and monster during catch")]
    public float monsterPlayerDistance = 2.3f;
    
    [Header("Spotlight Effect")]
    [Tooltip("Reference to SpotlightManager component")]
    public SpotlightManager spotlightManager;
    
    [Header("Audio Settings")]
    [Tooltip("Sound to play when caught")]
    public AudioClip catchSound;
    
    [Tooltip("Volume for catch sound")]
    [Range(0f, 1f)]
    public float catchSoundVolume = 1f;
    
    [Tooltip("Audio source for catch sounds")]
    public AudioSource audioSource;
    
    [Header("Animation")]
    [Tooltip("Reference to the player's Animator component")]
    public Animator playerAnimator;
    
    [Tooltip("Reference to the monster's Animator component")]
    public Animator monsterAnimator;
    
    [Header("Visual Effects")]
    [Tooltip("Screen fade effect (optional)")]
    public CanvasGroup screenFadeCanvas;
    
    [Tooltip("Duration of screen fade")]
    public float fadeDuration = 1f;
    
    [Header("Sequence Control")]
    [Tooltip("If true, the catch sequence will stop after completing")]
    public bool stopAfterCatch = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool isCatchSequenceActive = false;
    
    // Private variables
    private bool canCatch = true;
    private Vector3 playerRespawnPosition;
    private CatchCounter catchCounter;
    private Vector3 originalEnemyPosition; // Store original enemy position
    
    // Cache original player settings
    private bool originalPlayerCanMove;
    private bool originalCameraCanMove;
    private CursorLockMode originalCursorLockState;
    
    // CatchSequenceManager variables
    private Transform monsterTransform;
    private Transform playerTransform;
    private Transform playerCamera;
    private EnemyLightController enemyLightController;
    private Vector3 originalCameraPos;
    private bool isShaking = false;
    private Vector3 originalPlayerPos;
    private Light monsterSpotlight;
    private bool shouldStopSequence = false;

    private void Start()
    {
        // Auto-find components if not assigned
        if (enemyAI == null)
            enemyAI = GetComponent<enemyAi2>();
            
        if (playerController == null)
            playerController = FindObjectOfType<FirstPersonController>();
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find catch counter
        catchCounter = CatchCounter.Instance;
        
        // Store original enemy position
        originalEnemyPosition = transform.position;
        
        // Set up transforms for catch sequence
        SetupTransforms();
        
        // Create SpotlightManager if not assigned
        if (spotlightManager == null)
        {
            spotlightManager = GetComponent<SpotlightManager>();
            if (spotlightManager == null)
            {
                spotlightManager = gameObject.AddComponent<SpotlightManager>();
            }
        }
        
        // Store initial player respawn position
        if (playerController != null)
        {
            playerRespawnPosition = playerController.transform.position;
            
            // Create the PlayerSpawnManager if it doesn't exist (for fallback compatibility)
            if (PlayerSpawnManager.Instance == null && respawnPoint != null)
            {
                GameObject spawnManagerObj = new GameObject("PlayerSpawnManager");
                PlayerSpawnManager spawnManager = spawnManagerObj.AddComponent<PlayerSpawnManager>();
                spawnManager.manualSpawnPoint = respawnPoint;
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"CatchManager initialized. Enemy: {(enemyAI != null ? enemyAI.name : "None")}, Player: {(playerController != null ? playerController.name : "None")}");
        }
    }
    
    private void SetupTransforms()
    {
        monsterTransform = transform;
        
        if (playerController != null)
        {
            playerTransform = playerController.transform;
            playerCamera = playerController.playerCamera != null ? playerController.playerCamera.transform : playerController.GetComponentInChildren<Camera>().transform;
            
            if (playerCamera != null)
            {
                originalCameraPos = playerCamera.localPosition;
            }
            
            // Get the enemy light controller if it exists
            enemyLightController = GetComponentInChildren<EnemyLightController>();
            
            // Try to get player animator if not assigned
            if (playerAnimator == null)
            {
                playerAnimator = playerController.GetComponent<Animator>();
            }
            
            // Try to get monster animator if not assigned
            if (monsterAnimator == null)
            {
                monsterAnimator = GetComponent<Animator>();
            }
        }
    }

    private void Update()
    {
        // Only check for catch if not already in sequence and catch is enabled
        if (!isCatchSequenceActive && canCatch && enemyAI != null && playerController != null)
        {
            CheckForCatch();
        }
    }

    private void CheckForCatch()
    {
        // Calculate distance between enemy and player
        float distanceToPlayer = Vector3.Distance(transform.position, playerController.transform.position);
        
        // Check if within catch range
        if (distanceToPlayer <= catchRange)
        {
            // Additional checks to ensure catch is valid
            if (CanPerformCatch())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Player caught by {gameObject.name}!");
                }
                
                StartCatchSequence();
            }
        }
    }

    private bool CanPerformCatch()
    {
        // Check if player is in a safe zone
        if (checkSafe.IsPositionInSafeZone(playerController.transform.position))
        {
            if (showDebugInfo)
            {
                Debug.Log("Cannot catch player - player is in safe zone");
            }
            return false;
        }
        
        // Check if enemy is being watched (optional - might want to allow catch anyway)
        if (enemyAI.IsBeingWatched() && showDebugInfo)
        {
            Debug.Log("Player is watching enemy but catch will proceed");
        }
        
        return true;
    }

    public void StartCatchSequence()
    {
        if (isCatchSequenceActive)
            return;
            
        StartCoroutine(ExecuteCatchSequence());
    }

    private IEnumerator ExecuteCatchSequence()
    {
        isCatchSequenceActive = true;
        canCatch = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Starting advanced catch sequence...");
        }
        
        // Validate required transforms
        if (monsterTransform == null || playerTransform == null || playerCamera == null)
        {
            Debug.LogError("CatchManager: Missing required transforms for catch sequence!");
            yield break;
        }
        
        // Stop enemy AI normal behavior
        if (enemyAI != null)
        {
            enemyAI.SetCatchSequenceActive(true);
        }
        
        // Store original player settings and position
        StoreOriginalPlayerSettings();
        originalPlayerPos = playerTransform.position;
        
        // Set animation parameter on the monster animator
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool("playerCatch", true);
        }
        
        // Toggle enemy light if available and enabled
        if (enemyLightController != null && enemyLightController.enableOnCatch)
        {
            enemyLightController.ToggleLight(true);
        }
        // Use spotlight manager as fallback if no enemy light controller found
        else if (spotlightManager != null && spotlightManager.enableSpotlightOnCatch)
        {
            monsterSpotlight = spotlightManager.CreateSpotlight(monsterTransform);
        }
        
        // Disable player movement
        DisablePlayerMovement();
        
        // Start screen fade if available
        if (screenFadeCanvas != null)
        {
            StartCoroutine(FadeScreen(true));
        }
        
        // Turn the monster to face the player
        StartCoroutine(TurnMonsterToFacePlayer());
        
        // Wait a bit for the monster to start turning
        yield return new WaitForSeconds(0.2f);
        
        // Make player camera look at the monster
        StartCoroutine(TurnCameraToFaceMonster());
        
        // Start camera shake
        StartCoroutine(ShakeCamera());
        
        // Lift player slightly - store the coroutine so we can stop it if needed
        Coroutine liftCoroutine = StartCoroutine(LiftPlayer());
        
        // Play catch sound
        PlayCatchSound();
        
        // Wait for the sequence to complete
        yield return new WaitForSeconds(Mathf.Max(shakeDuration, playerLiftDuration));
        
        // Turn off spotlight if it was created and no enemy light controller exists
        if (enemyLightController == null && spotlightManager != null && spotlightManager.enableSpotlightOnCatch)
        {
            spotlightManager.RemoveSpotlight();
        }
        
        // Increment catch counter
        if (catchCounter != null)
        {
            catchCounter.IncrementCatchCount();
        }
        
        // If stopAfterCatch is true, stop all coroutines and clean up
        if (stopAfterCatch)
        {
            shouldStopSequence = true;
            StopCoroutine(liftCoroutine);
        }
        
        // Handle player respawn or game reset
        if (respawnPlayerOnCatch)
        {
            yield return StartCoroutine(ResetPlayerAfterDelay());
        }
        else
        {
            yield return new WaitForSeconds(respawnDelay);
        }
        
        // Fade screen back in
        if (screenFadeCanvas != null)
        {
            StartCoroutine(FadeScreen(false));
        }
        
        // Reset everything
        ResetCatchSequence();
        
        if (showDebugInfo)
        {
            Debug.Log("Advanced catch sequence completed");
        }
    }

    private void StoreOriginalPlayerSettings()
    {
        if (playerController != null)
        {
            originalPlayerCanMove = playerController.playerCanMove;
            originalCameraCanMove = playerController.cameraCanMove;
            originalCursorLockState = Cursor.lockState;
        }
    }

    private void DisablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.playerCanMove = false;
            playerController.cameraCanMove = false;
            
            // Stop player movement by setting velocity to zero
            Rigidbody playerRb = playerController.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void RestorePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.playerCanMove = originalPlayerCanMove;
            playerController.cameraCanMove = originalCameraCanMove;
            Cursor.lockState = originalCursorLockState;
        }
    }

    private void PlayCatchSound()
    {
        if (catchSound != null)
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(catchSound);
            }
            else
            {
                AudioSource.PlayClipAtPoint(catchSound, playerTransform.position, catchSoundVolume);
            }
        }
    }
    
    // Turn monster to face player
    private IEnumerator TurnMonsterToFacePlayer()
    {
        float turnDuration = 1.0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < turnDuration && !shouldStopSequence)
        {
            // Calculate direction to player
            Vector3 targetDirection = playerTransform.position - monsterTransform.position;
            targetDirection.y = 0f; // Keep on same horizontal plane
            
            // Create the rotation to face the player
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            
            // Smoothly rotate towards target
            monsterTransform.rotation = Quaternion.Slerp(
                monsterTransform.rotation, 
                targetRotation, 
                monsterTurnSpeed * Time.deltaTime
            );
               
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (!shouldStopSequence)
        {
            // Ensure exact final rotation facing player
            Vector3 finalDirection = playerTransform.position - monsterTransform.position;
            finalDirection.y = 0f;
            monsterTransform.rotation = Quaternion.LookRotation(finalDirection);
        }
    }
    
    // Turn player camera to face monster
    private IEnumerator TurnCameraToFaceMonster()
    {
        // Disable player camera control
        if (playerController != null)
        {
            playerController.cameraCanMove = false;
        }
        
        float turnDuration = 1.5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < turnDuration && !shouldStopSequence)
        {
            // Calculate position to look at on the monster (incorporating Y offset)
            Vector3 monsterLookTarget = monsterTransform.position + new Vector3(0, monsterFaceYOffset, 0);
            
            // Calculate direction from player to monster's face
            Vector3 directionToMonster = monsterLookTarget - playerCamera.position;
            
            // Create target rotation - this makes the camera look at the monster
            Quaternion targetRotation = Quaternion.LookRotation(directionToMonster);
            
            // Split rotation between player body (yaw) and camera (pitch)
            float targetYaw = targetRotation.eulerAngles.y;
            float targetPitch = targetRotation.eulerAngles.x;
            
            // Need to normalize pitch angle for proper interpolation
            if (targetPitch > 180f)
                targetPitch -= 360f;
            
            // Rotate player body (horizontal rotation)
            playerTransform.rotation = Quaternion.Slerp(
                playerTransform.rotation,
                Quaternion.Euler(0f, targetYaw, 0f),
                cameraRotationSpeed * Time.deltaTime
            );
            
            // Rotate camera (vertical rotation)
            playerCamera.localRotation = Quaternion.Slerp(
                playerCamera.localRotation,
                Quaternion.Euler(targetPitch, 0f, 0f),
                cameraRotationSpeed * Time.deltaTime
            );
               
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    // Camera shake effect
    private IEnumerator ShakeCamera()
    {
        isShaking = true;
        float elapsed = 0.0f;
        
        while (elapsed < shakeDuration && !shouldStopSequence)
        {
            // Calculate random offset
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount),
                Random.Range(-shakeAmount, shakeAmount)
            );
            
            // Apply offset to camera
            playerCamera.localPosition = originalCameraPos + shakeOffset;
               
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Reset camera position
        if (playerCamera != null)
        {
            playerCamera.localPosition = originalCameraPos;
        }
        isShaking = false;
    }
    
    // Lift player slightly during catch sequence
    private IEnumerator LiftPlayer()
    {
        // Get direction toward monster for angled lifting
        Vector3 directionToMonster = (monsterTransform.position - playerTransform.position).normalized;
        directionToMonster.y = 0; // Keep horizontal direction only
        
        // Calculate lift vector - combination of up and slight angle toward monster
        Vector3 liftVector = Vector3.up + (directionToMonster * 0.2f);
        liftVector = liftVector.normalized * playerLiftAmount;
        
        // Calculate position that maintains the desired distance from the monster
        Vector3 monsterPosition = monsterTransform.position;
        monsterPosition.y = originalPlayerPos.y; // Match player's original height
        
        // Position that is 'monsterPlayerDistance' units away from the monster in the opposite direction
        Vector3 targetXZPosition = monsterPosition - (directionToMonster * monsterPlayerDistance);
        
        // Combine the XZ positioning with the Y lift
        Vector3 liftedPosition = new Vector3(
            targetXZPosition.x,
            originalPlayerPos.y + liftVector.y,
            targetXZPosition.z
        );
        
        // Apply lift to position
        playerTransform.position = liftedPosition;
        
        // Maintain the position during the catch sequence
        float duration = playerLiftDuration;
        float elapsed = 0f;
        
        while (elapsed < duration && !shouldStopSequence)
        {
            // Skip position updates if we should stop the sequence
            if (shouldStopSequence)
            {
                yield break;
            }
            
            // Update direction to monster (in case monster moves)
            directionToMonster = (monsterTransform.position - playerTransform.position).normalized;
            directionToMonster.y = 0;
            
            // Recalculate position to maintain consistent distance
            monsterPosition = monsterTransform.position;
            monsterPosition.y = originalPlayerPos.y;
            targetXZPosition = monsterPosition - (directionToMonster * monsterPlayerDistance);
            
            // Set position maintaining both the distance to monster and the lifted Y position
            playerTransform.position = new Vector3(
                targetXZPosition.x,
                liftedPosition.y,
                targetXZPosition.z
            );
               
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FadeScreen(bool fadeOut)
    {
        if (screenFadeCanvas == null)
            yield break;
            
        float startAlpha = fadeOut ? 0f : 1f;
        float endAlpha = fadeOut ? 1f : 0f;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            screenFadeCanvas.alpha = alpha;
            yield return null;
        }
        
        screenFadeCanvas.alpha = endAlpha;
    }

    private IEnumerator ResetPlayerAfterDelay()
    {
        // Stop the enemy AI immediately when caught
        if (enemyAI != null)
        {
            // Disable the enemy AI component to stop all chasing behavior
            enemyAI.enabled = false;
            
            // Optional: Freeze the enemy's position and rotation by setting rigidbody to kinematic if available
            Rigidbody enemyRb = GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = true;
                enemyRb.velocity = Vector3.zero;
                enemyRb.angularVelocity = Vector3.zero;
            }
            
            // Optional: Stop any ongoing animations that might be playing
            if (monsterAnimator != null && monsterAnimator.enabled)
            {
                // Set animation speed to 1
                monsterAnimator.speed = 1;
            }
        }
        
        // Wait for the specified delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Reset player position and rotation
        if (playerController != null)
        {
            // Get player's spawn position
            Vector3 spawnPos;
            Quaternion spawnRot;
            
            if (PlayerSpawnManager.Instance != null)
            {
                spawnPos = PlayerSpawnManager.Instance.GetSpawnPosition();
                spawnRot = PlayerSpawnManager.Instance.GetSpawnRotation();
                if (showDebugInfo)
                    Debug.Log($"Resetting player to spawn position from PlayerSpawnManager: {spawnPos}");
            }
            else if (respawnPoint != null)
            {
                // Fallback to respawn point if set
                spawnPos = respawnPoint.position;
                spawnRot = respawnPoint.rotation;
                if (showDebugInfo)
                    Debug.Log($"Resetting player to respawn point: {spawnPos}");
            }
            else
            {
                // Use stored initial position
                spawnPos = playerRespawnPosition;
                spawnRot = Quaternion.identity;
                if (showDebugInfo)
                    Debug.Log($"Resetting player to initial spawn position: {spawnPos}");
            }
            
            // Get CharacterController if it exists
            CharacterController charController = playerController.GetComponent<CharacterController>();
            if (charController != null)
            {
                // Disable CharacterController temporarily to avoid collision issues during teleport
                charController.enabled = false;
                playerController.transform.position = spawnPos;
                playerController.transform.rotation = spawnRot;
                charController.enabled = true;
            }
            else
            {
                // Direct position reset if no CharacterController
                playerController.transform.position = spawnPos;
                playerController.transform.rotation = spawnRot;
            }
            
            // Reset player velocity
            Rigidbody playerRb = playerController.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }
            
            if (showDebugInfo)
                Debug.Log("Player has been reset to spawn position!");
        }
        
        // Re-enable enemy AI and reset its state
        if (enemyAI != null)
        {
            // Reset enemy state first to ensure proper initialization
            enemyAI.ResetState();
            
            // Reset the playerCatch animation parameter
            if (monsterAnimator != null)
            {
                monsterAnimator.SetBool("playerCatch", false);
                if (showDebugInfo)
                    Debug.Log("Reset playerCatch animation parameter to false");
            }
            
            // Return to normal AI behavior
            enemyAI.enabled = true;
            
            // Re-enable rigidbody physics
            Rigidbody enemyRb = GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = false;
            }
            
            // Reset enemy to original position
            transform.position = originalEnemyPosition;
            
            if (showDebugInfo)
                Debug.Log($"Enemy has been reset and teleported to original position: {originalEnemyPosition}");
        }
    }

    private void RespawnPlayer()
    {
        // This method is now handled by ResetPlayerAfterDelay
        // Keeping for backward compatibility - just call the reset function
        StartCoroutine(ResetPlayerAfterDelay());
    }

    private void ResetCatchSequence()
    {
        // Stop any sequence effects
        shouldStopSequence = true;
        
        // Restore player movement
        RestorePlayerMovement();
        
        // Reset camera position if needed
        if (playerCamera != null)
        {
            playerCamera.localPosition = originalCameraPos;
        }
        
        // Reset monster animation
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool("playerCatch", false);
        }
        
        // Reset enemy AI
        if (enemyAI != null)
        {
            enemyAI.SetCatchSequenceActive(false);
            enemyAI.ResetState();
        }
        
        // Turn off lights
        if (enemyLightController != null)
        {
            enemyLightController.ToggleLight(false);
        }
        
        // Allow catching again after a brief delay
        StartCoroutine(ReenableCatchAfterDelay(2f));
        
        isCatchSequenceActive = false;
        shouldStopSequence = false;
    }

    // Method to forcefully stop all sequence effects
    public void StopSequence()
    {
        shouldStopSequence = true;
        StopAllCoroutines();
        
        // Reset camera position if needed
        if (playerCamera != null)
        {
            playerCamera.localPosition = originalCameraPos;
        }
        
        // Restore player control
        RestorePlayerMovement();
        
        isCatchSequenceActive = false;
    }

    private IEnumerator ReenableCatchAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canCatch = true;
        
        if (showDebugInfo)
        {
            Debug.Log("Catch enabled again");
        }
    }

    // Public methods for external control
    public void SetCatchEnabled(bool enabled)
    {
        canCatch = enabled;
        if (showDebugInfo)
        {
            Debug.Log($"Catch enabled set to: {enabled}");
        }
    }

    public bool IsCatchSequenceActive()
    {
        return isCatchSequenceActive;
    }

    public void SetRespawnPosition(Vector3 position)
    {
        playerRespawnPosition = position;
        if (showDebugInfo)
        {
            Debug.Log($"Respawn position updated to: {position}");
        }
        
        // Also update PlayerSpawnManager if it exists
        if (PlayerSpawnManager.Instance != null)
        {
            // Create a temporary transform for the position
            GameObject tempObj = new GameObject("TempSpawnPoint");
            tempObj.transform.position = position;
            PlayerSpawnManager.Instance.manualSpawnPoint = tempObj.transform;
        }
    }
    
    // Method to set respawn point transform
    public void SetRespawnPoint(Transform point)
    {
        respawnPoint = point;
        if (point != null)
        {
            playerRespawnPosition = point.position;
            if (showDebugInfo)
            {
                Debug.Log($"Respawn point updated to: {point.name} at {point.position}");
            }
        }
    }

    // Force trigger catch (for testing or special events)
    public void ForceCatch()
    {
        if (!isCatchSequenceActive && playerController != null)
        {
            if (showDebugInfo)
            {
                Debug.Log("Force catch triggered");
            }
            StartCatchSequence();
        }
    }

    // Collision detection as backup method
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        FirstPersonController detectedPlayer = other.GetComponent<FirstPersonController>();
        if (detectedPlayer != null && detectedPlayer == playerController && !isCatchSequenceActive && canCatch)
        {
            if (CanPerformCatch())
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Player caught by trigger collision with {gameObject.name}!");
                }
                StartCatchSequence();
            }
        }
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (showDebugInfo)
        {
            // Draw catch range
            Gizmos.color = isCatchSequenceActive ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, catchRange);
            
            // Draw line to player if in range
            if (playerController != null)
            {
                float distance = Vector3.Distance(transform.position, playerController.transform.position);
                if (distance <= catchRange)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, playerController.transform.position);
                }
            }
        }
    }
}
