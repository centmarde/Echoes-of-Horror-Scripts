using System.Collections;
using UnityEngine;

public class CatchSequenceManager : MonoBehaviour
{
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
    
    [Header("Spotlight Effect")]
    [Tooltip("Reference to SpotlightManager component")]
    public SpotlightManager spotlightManager;
    
    [Header("Audio Settings")]
    [Tooltip("Sound to play when caught")]
    public AudioClip catchSound;
    
    [Tooltip("Volume for catch sound")]
    [Range(0f, 1f)]
    public float catchSoundVolume = 1f;
    
    [Header("Monster Player Distance")]
    [Tooltip("Distance between player and monster during catch")]
    public float monsterPlayerDistance = 2.3f;
    
    [Header("Animation")]
    [Tooltip("Reference to the player's Animator component")]
    public Animator playerAnimator;
    
    [Tooltip("Reference to the monster's Animator component")]
    public Animator monsterAnimator;
    
    [Header("Sequence Control")]
    [Tooltip("If true, the catch sequence will stop after completing")]
    public bool stopAfterCatch = false;
    
    private Transform monsterTransform;
    private Transform playerTransform;
    private Transform playerCamera;
    private FirstPersonController playerController;
    private enemyAi enemyAIComponent;
    private EnemyLightController enemyLightController;
    private Vector3 originalCameraPos;
    private bool isShaking = false;
    private Vector3 originalPlayerPos;
    private Light monsterSpotlight;
    private bool shouldStopSequence = false;
    
    private void Awake()
    {
        // Create SpotlightManager if not assigned
        if (spotlightManager == null)
        {
            spotlightManager = GetComponent<SpotlightManager>();
            if (spotlightManager == null)
            {
                spotlightManager = gameObject.AddComponent<SpotlightManager>();
            }
        }
    }
    
    // Initialize with required transforms
    public void Setup(Transform monster, Transform player, Transform camera, FirstPersonController controller)
    {
        monsterTransform = monster;
        playerTransform = player;
        playerCamera = camera;
        playerController = controller;
        originalCameraPos = playerCamera.localPosition;
        originalPlayerPos = playerTransform.position;
        enemyAIComponent = monster.GetComponent<enemyAi>();
        
        // Get the enemy light controller if it exists
        enemyLightController = monster.GetComponentInChildren<EnemyLightController>();
        
        // Try to get player animator if not assigned
        if (playerAnimator == null && player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }
        
        // Try to get monster animator if not assigned
        if (monsterAnimator == null && monster != null)
        {
            monsterAnimator = monster.GetComponent<Animator>();
        }
    }
    
    // Execute the catch sequence
    public IEnumerator ExecuteCatchSequence()
    {
        if (monsterTransform == null || playerTransform == null || playerCamera == null)
        {
            Debug.LogError("CatchSequenceManager: Missing required transforms for catch sequence!");
            yield break;
        }
        
        // Store original player position
        originalPlayerPos = playerTransform.position;
        
        // Set animation parameter on the monster animator, not the player
        if (monsterAnimator != null)
        {
            monsterAnimator.SetBool("playerCatch", true);
           
           // Ensure the animation continues to play by disabling any interruptions
           if (enemyAIComponent != null)
           {
               // Tell the enemy AI that catch sequence is active
               enemyAIComponent.SetCatchSequenceActive(true);
           }
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
        
        // Play catch sound if available
        if (catchSound != null)
        {
            AudioSource.PlayClipAtPoint(catchSound, playerTransform.position, catchSoundVolume);
        }
        
        // Wait for the sequence to complete
        yield return new WaitForSeconds(Mathf.Max(shakeDuration, 1.5f));
        
        // Turn off spotlight if it was created and no enemy light controller exists
        if (enemyLightController == null && spotlightManager != null && spotlightManager.enableSpotlightOnCatch)
        {
            spotlightManager.RemoveSpotlight();
        }
           
        // If stopAfterCatch is true, stop all coroutines and clean up
        if (stopAfterCatch)
        {
            shouldStopSequence = true;
            StopCoroutine(liftCoroutine);
            
            // Destroy this component when done
            Destroy(this);
        }
    }
    
    // Turn monster to face player
    private IEnumerator TurnMonsterToFacePlayer()
    {
        float turnDuration = 1.0f;
        float elapsedTime = 0f;
        
        while (elapsedTime < turnDuration)
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
        
        // Ensure exact final rotation facing player
        Vector3 finalDirection = playerTransform.position - monsterTransform.position;
        finalDirection.y = 0f;
        monsterTransform.rotation = Quaternion.LookRotation(finalDirection);
    }
    
    // Turn player camera to face monster
    private IEnumerator TurnCameraToFaceMonster()
    {
        // Disable player camera control
        playerController.cameraCanMove = false;
        
        float turnDuration = 1.5f;
        float elapsedTime = 0f;
        
        // Get the initial rotations
        Quaternion initialPlayerBodyRotation = playerTransform.rotation;
        Quaternion initialCameraLocalRotation = playerCamera.localRotation;
        
        while (elapsedTime < turnDuration)
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
        
        while (elapsed < shakeDuration)
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
        playerCamera.localPosition = originalCameraPos;
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
    }
}
