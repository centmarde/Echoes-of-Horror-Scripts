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
    
    [Header("Scene Restart")]
    [Tooltip("Whether to restart the scene when player is caught")]
    public bool restartSceneOnCatch = true;
    
    [Tooltip("Delay before restarting the scene (in seconds)")]
    public float restartDelay = 2.0f;
    
    // Private references
    private enemyAi enemyAI;
    private FirstPersonController playerController;
    private bool isPlayerFrozen = false;
    private Coroutine freezeCoroutine;
    
    private void Awake()
    {
        // Get reference to the enemyAI component
        enemyAI = GetComponent<enemyAi>();
    }
    
    private void Start()
    {
        // Find the player's FirstPersonController component
        if (enemyAI.player != null)
        {
            playerController = enemyAI.player.GetComponent<FirstPersonController>();
            if (playerController == null)
            {
                Debug.LogError("PlayerFreezeController could not find FirstPersonController on player. Player freezing will not work.");
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
        
        Debug.Log("Player has been frozen!");
        
        // Restart the scene if enabled
        if (restartSceneOnCatch)
        {
            StartCoroutine(RestartSceneAfterDelay());
        }
        // Only start the unfreeze timer if we're not restarting the scene
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
    
    private IEnumerator RestartSceneAfterDelay()
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
                // Set animation speed to 0 or pause it
                enemyAnimator.speed = 1;
            }
        }
        
        // Add catch sequence manager to handle the capture animation
        CatchSequenceManager sequenceManager = gameObject.AddComponent<CatchSequenceManager>();
        
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
        yield return new WaitForSeconds(restartDelay);
        
        // Get the current scene and reload it
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        
        Debug.Log("Scene restarted after player was caught!");
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
