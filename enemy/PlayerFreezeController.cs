using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(enemyAi))]
public class PlayerFreezeController : MonoBehaviour
{
    [Header("Player Freeze Settings")]
    [Tooltip("Distance at which to freeze the player")]
    public float freezeDistance = 1.5f;
    
    [Tooltip("How frequently to check if player should be frozen (in seconds)")]
    public float checkFrequency = 0.2f;
    
    [Tooltip("Freeze duration (in seconds). Set to 0 for infinite freeze")]
    public float freezeDuration = 0f;
    
    [Tooltip("If true, will use collision detection instead of distance checking")]
    public bool useCollisionDetection = true;
    
    [Header("Events")]
    public UnityEvent onPlayerFreeze;
    public UnityEvent onPlayerUnfreeze;
    
    [Header("Player Reset")]
    [Tooltip("Whether to reset the player position when caught")]
    public bool resetPlayerOnCatch = true;
    
    [Tooltip("Delay before resetting the player (in seconds)")]
    public float resetDelay = 2.0f;
    
    [Tooltip("Transform to use as the respawn position (optional)")]
    public Transform respawnPoint;
    
    // Private references
    private enemyAi enemyAI;
    private FirstPersonController playerController;
    private bool isPlayerFrozen = false;
    private Coroutine freezeCoroutine;
    private Vector3 originalEnemyPosition; // Add variable for original position
    
    private void Awake()
    {
        // Get reference to the enemyAI component
        enemyAI = GetComponent<enemyAi>();
    }
    
    private void Start()
    {
        // Store the original enemy position
        originalEnemyPosition = transform.position;
        
        // Find the player's FirstPersonController component
        if (enemyAI.player != null)
        {
            playerController = enemyAI.player.GetComponent<FirstPersonController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerFreezeController could not find FirstPersonController on player. Player freezing will not work.");
            }
            else
            {
                // Create the PlayerSpawnManager if it doesn't exist
                if (PlayerSpawnManager.Instance == null)
                {
                    GameObject spawnManagerObj = new GameObject("PlayerSpawnManager");
                    PlayerSpawnManager spawnManager = spawnManagerObj.AddComponent<PlayerSpawnManager>();
                    
                    // If respawnPoint is set, assign it to the spawn manager
                    if (respawnPoint != null)
                    {
                        spawnManager.manualSpawnPoint = respawnPoint;
                    }
                }
                else if (respawnPoint != null)
                {
                    // If PlayerSpawnManager already exists but we have a specified respawnPoint, update it
                    PlayerSpawnManager.Instance.manualSpawnPoint = respawnPoint;
                }
            }
        }
        else
        {
            Debug.LogError("PlayerFreezeController: enemyAI.player reference is null. Player freezing will not work.");
        }
        
        // Start the periodic distance check if not using collision detection
        if (!useCollisionDetection)
        {
            StartCoroutine(CheckPlayerDistance());
        }
    }
    
    private IEnumerator CheckPlayerDistance()
    {
        while (true)
        {
            if (enemyAI.player != null && playerController != null && !isPlayerFrozen)
            {
                float distance = Vector3.Distance(transform.position, enemyAI.player.position);
                
                // If enemy is close enough to the player, freeze the player
                if (distance <= freezeDistance)
                {
                    FreezePlayer();
                }
            }
            
            yield return new WaitForSeconds(checkFrequency);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!useCollisionDetection) return;
        
        // Check if we collided with the player
        if (playerController != null && collision.gameObject == playerController.gameObject)
        {
            FreezePlayer();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!useCollisionDetection) return;
        
        // Check if we entered the player's trigger
        if (playerController != null && other.gameObject == playerController.gameObject)
        {
            FreezePlayer();
        }
    }
    
    public void FreezePlayer()
    {
        if (isPlayerFrozen || playerController == null) return;
        
        // Disable player movement and camera controls
        playerController.playerCanMove = false;
        playerController.cameraCanMove = false;
        isPlayerFrozen = true;
        
        // Trigger the freeze event
        onPlayerFreeze?.Invoke();
        
        // Increment the catch counter
        if (CatchCounter.Instance != null)
        {
            CatchCounter.Instance.IncrementCatchCount();
        }
        
        Debug.Log("Player has been frozen!");
        
        // Reset player if enabled
        if (resetPlayerOnCatch)
        {
            StartCoroutine(ResetPlayerAfterDelay());
        }
        // Only start the unfreeze timer if we're not resetting the player
        else if (freezeDuration > 0)
        {
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
            }
            freezeCoroutine = StartCoroutine(UnfreezeAfterDelay());
        }
    }
    
    public void UnfreezePlayer()
    {
        if (!isPlayerFrozen || playerController == null) return;
        
        // Re-enable player movement and camera controls
        playerController.playerCanMove = true;
        playerController.cameraCanMove = true;
        isPlayerFrozen = false;
        
        // Trigger the unfreeze event
        onPlayerUnfreeze?.Invoke();
        
        Debug.Log("Player has been unfrozen!");
    }
    
    private IEnumerator UnfreezeAfterDelay()
    {
        yield return new WaitForSeconds(freezeDuration);
        UnfreezePlayer();
    }
    
    private IEnumerator ResetPlayerAfterDelay()
    {
        // Stop the enemy AI immediately when caught
        if (enemyAI != null)
        {
            // Disable the enemy AI component to stop all chasing behavior
            enemyAI.enabled = false;
            
            // Optional: Freeze the enemy's position and rotation by setting rigidbody to kinematic if available
            Rigidbody enemyRb = enemyAI.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = true;
                enemyRb.velocity = Vector3.zero;
                enemyRb.angularVelocity = Vector3.zero;
            }
            
            // Optional: Stop any ongoing animations that might be playing
            Animator enemyAnimator = enemyAI.GetComponent<Animator>();
            if (enemyAnimator != null && enemyAnimator.enabled)
            {
                // Set animation speed to 1
                enemyAnimator.speed = 1;
            }
        }
        
        // Add catch sequence manager to handle the capture animation
        CatchSequenceManager sequenceManager = gameObject.AddComponent<CatchSequenceManager>();
        sequenceManager.stopAfterCatch = true;
        
        // Setup the sequence manager with necessary references
        if (playerController != null && playerController.playerCamera != null)
        {
            sequenceManager.Setup(
                transform,                        // monster transform
                playerController.transform,       // player transform
                playerController.playerCamera.transform,  // camera transform
                playerController                 // player controller reference
            );
            
            // Execute catch sequence
            yield return StartCoroutine(sequenceManager.ExecuteCatchSequence());
        }
        
        // Wait for the specified delay after sequence completes
        yield return new WaitForSeconds(resetDelay);
        
        // Reset player position and rotation
        if (playerController != null)
        {
            // Get player's original spawn position from the PlayerSpawnManager
            Vector3 spawnPos;
            Quaternion spawnRot;
            
            if (PlayerSpawnManager.Instance != null)
            {
                spawnPos = PlayerSpawnManager.Instance.GetSpawnPosition();
                spawnRot = PlayerSpawnManager.Instance.GetSpawnRotation();
                Debug.Log($"Resetting player to spawn position from PlayerSpawnManager: {spawnPos}");
            }
            else if (respawnPoint != null)
            {
                // Fallback to respawn point if set
                spawnPos = respawnPoint.position;
                spawnRot = respawnPoint.rotation;
                Debug.Log($"Resetting player to respawn point: {spawnPos}");
            }
            else
            {
                // Last resort: Use a safe position away from the enemy
                spawnPos = transform.position + transform.forward * -10f;
                spawnRot = Quaternion.identity;
                Debug.LogWarning("No spawn position found, using fallback position!");
            }
            
            // Ensure CatchSequenceManager doesn't affect player anymore
            if (sequenceManager != null)
            {
                sequenceManager.StopSequence();
                Destroy(sequenceManager);
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
            
            // Re-enable the player controller
            UnfreezePlayer();
            
            Debug.Log("Player has been reset to spawn position!");
        }
        
        // Re-enable enemy AI and reset its state
        if (enemyAI != null)
        {
            // Reset enemy state first to ensure proper initialization
            enemyAI.ResetState();
            
            // Reset the playerCatch animation parameter
            Animator enemyAnimator = enemyAI.GetComponent<Animator>();
            if (enemyAnimator != null)
            {
                enemyAnimator.SetBool("playerCatch", false);
                Debug.Log("Reset playerCatch animation parameter to false");
            }
            
            // Return to normal AI behavior
            enemyAI.enabled = true;
            
            // Re-enable rigidbody physics
            Rigidbody enemyRb = enemyAI.GetComponent<Rigidbody>();
            if (enemyRb != null)
            {
                enemyRb.isKinematic = false;
            }
            
            // Apply the original position instead of newEnemyPosition
            enemyAI.transform.position = originalEnemyPosition;
            
            Debug.Log($"Enemy has been reset and teleported to original position: {originalEnemyPosition}");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!useCollisionDetection)
        {
            // Draw the freeze radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, freezeDistance);
        }
    }
}
