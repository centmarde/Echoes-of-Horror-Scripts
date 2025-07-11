using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Tooltip("Key to trigger enemy rotation")]
    public KeyCode rotationKey = KeyCode.R;

    [Tooltip("Max distance to affect enemies")]
    public float maxAffectDistance = 20f;

    [Tooltip("Rotation speed when turning enemies")]
    public float rotationSpeed = 5f;

    [Tooltip("Duration of the rotation effect in seconds")]
    public float effectDuration = 2f;

    [Tooltip("Enemy tag to look for (leave empty for all enemyAi components)")]
    public string enemyTag = "Enemy";

    [Header("Rotation Ability Cooldown")]
    [Tooltip("Cooldown time in seconds before rotation can be used again")]
    public float rotationCooldown = 10f;
    
    [Tooltip("Visual feedback UI element for cooldown (optional)")]
    public UnityEngine.UI.Image cooldownIndicator;

    [Header("Flashlight Effect")]
    [Tooltip("Whether the enemy can be frozen by flashlight")]
    public bool canBeFrozenByFlashlight = true;

    [Tooltip("Visual effect prefab to show when frozen (optional)")]
    public GameObject frozenEffectPrefab;

    [Header("Dimension Teleportation")]
    [Tooltip("Whether the enemy can teleport to another dimension when hit by flashlight")]
    public bool canTeleportOnFlashlight = true;
    
    [Tooltip("Minimum time in seconds the flashlight must be pointing at the enemy to trigger teleportation")]
    public float teleportExposureTime = 1.5f;
    
    [Tooltip("Teleport destination (empty = random position within teleportRadius)")]
    public Transform teleportDestination;
    
    [Tooltip("Radius for random teleportation if no destination is set")]
    public float teleportRadius = 30f;
    
    [Tooltip("Minimum distance from the player for random teleportation")]
    public float minTeleportDistanceFromPlayer = 15f;
    
    [Tooltip("Cooldown time in seconds before teleportation can occur again")]
    public float teleportCooldown = 15f;
    
    [Tooltip("Visual effect prefab to show when teleporting (optional)")]
    public GameObject teleportEffectPrefab;

    // Internal variables
    private List<enemyAi> activeRotatingEnemies = new List<enemyAi>();
    private Dictionary<enemyAi, Coroutine> activeRotations = new Dictionary<enemyAi, Coroutine>();

    // Internal variables for frozen state
    private bool isFrozen = false;
    private GameObject activeFrozenEffect = null;
    private enemyAi enemyAiComponent;

    // Internal variables for teleportation
    private float flashlightExposureTimer = 0f;
    private bool isExposedToFlashlight = false;
    private float lastTeleportTime = -1000f; // Initialize to allow immediate first use
    private bool isTeleporting = false;

    // Original movement speed values to restore after unfreezing
    private float originalNormalSpeed;
    private float originalChaseSpeed;
    private float originalSlowSpeed;
    private float originalRoamSpeed;
    
    // Cooldown tracking variables
    private float lastRotationTime = -1000f; // Initialize to allow immediate first use
    private bool isRotationOnCooldown = false;

    // Reference to player transform and flashlight
    private Transform playerTransform;
    private flashlight playerFlashlight;

    void Start()
    {
        // Get reference to the enemyAi component
        enemyAiComponent = GetComponent<enemyAi>();

        // Store original movement speeds if component exists
        if (enemyAiComponent != null)
        {
            StoreOriginalSpeeds();
        }
        
        // Initialize cooldown indicator if assigned
        if (cooldownIndicator != null)
        {
            cooldownIndicator.fillAmount = 0;
            cooldownIndicator.gameObject.SetActive(false);
        }
        
        // Find player and flashlight
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerFlashlight = playerObj.GetComponentInChildren<flashlight>();
            if (playerFlashlight == null)
            {
                Debug.LogWarning("Couldn't find flashlight component on player for teleport functionality.");
            }
        }
        else
        {
            Debug.LogWarning("Couldn't find player for teleport functionality.");
        }
    }

    void Update()
    {
        // Handle cooldown logic
        HandleRotationCooldown();
        
        // Check for key press only if not on cooldown
        if (Input.GetKeyDown(rotationKey) && !isRotationOnCooldown)
        {
            TriggerEnemyRotation();
            // Start cooldown
            lastRotationTime = Time.time;
            isRotationOnCooldown = true;
            
            // Show and initialize cooldown indicator
            if (cooldownIndicator != null)
            {
                cooldownIndicator.fillAmount = 1;
                cooldownIndicator.gameObject.SetActive(true);
            }
        }
        
        // Check if flashlight is shining on enemy
        if (canTeleportOnFlashlight && playerFlashlight != null && !isTeleporting && Time.time >= lastTeleportTime + teleportCooldown)
        {
            CheckFlashlightExposure();
        }
    }
    
    // Handle checking if the enemy is exposed to the flashlight
    private void CheckFlashlightExposure()
    {
        if (playerFlashlight.IsFlashlightOn() && playerTransform != null)
        {
            // Calculate direction from player to enemy
            Vector3 playerToEnemy = transform.position - playerTransform.position;
            float distanceToPlayer = playerToEnemy.magnitude;
            
            if (distanceToPlayer <= maxAffectDistance)
            {
                // Normalize direction
                Vector3 playerToEnemyDir = playerToEnemy.normalized;
                
                // Get player's forward direction (where they're looking)
                Vector3 playerForward = playerTransform.forward;
                
                // Calculate the angle between player forward and direction to enemy
                float angle = Vector3.Angle(playerForward, playerToEnemyDir);
                
                // Check if enemy is within the flashlight cone (assuming a 30-degree cone)
                if (angle <= 30f)
                {
                    // Cast a ray from player to enemy to check for obstacles
                    RaycastHit hit;
                    if (Physics.Raycast(playerTransform.position + Vector3.up * 1.5f, playerToEnemyDir, out hit, distanceToPlayer))
                    {
                        // If the ray hit this enemy first, they're exposed to the flashlight
                        if (hit.transform == this.transform || hit.transform.IsChildOf(this.transform))
                        {
                            // Start or continue counting exposure time
                            isExposedToFlashlight = true;
                            flashlightExposureTimer += Time.deltaTime;
                            
                            // If exposed long enough, trigger teleportation
                            if (flashlightExposureTimer >= teleportExposureTime)
                            {
                                TriggerTeleportation();
                            }
                            return;
                        }
                    }
                }
            }
        }
        
        // If we reach here, the enemy is not exposed to the flashlight
        isExposedToFlashlight = false;
        flashlightExposureTimer = 0f;
    }
    
    // Trigger teleportation to another dimension
    private void TriggerTeleportation()
    {
        // Reset timer
        flashlightExposureTimer = 0f;
        
        // Set cooldown
        lastTeleportTime = Time.time;
        
        // Start teleport effect and movement
        StartCoroutine(TeleportEnemyToAnotherDimension());
    }
    
    // Coroutine to handle teleportation with visual effects
    private IEnumerator TeleportEnemyToAnotherDimension()
    {
        isTeleporting = true;
        
        // Spawn teleport effect if available
        if (teleportEffectPrefab != null)
        {
            GameObject effect = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Pause the enemy AI movement
        if (enemyAiComponent != null)
        {
            SetAllSpeeds(0f);
        }
        
        // Optional: Make the enemy fade out
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            // Store the original materials to restore later
            Material[] originalMaterials = renderer.materials;
            
            // Create temporary materials we can modify
            Material[] fadeMaterials = new Material[originalMaterials.Length];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                fadeMaterials[i] = new Material(originalMaterials[i]);
                renderer.materials = fadeMaterials;
            }
            
            // Fade out over time
            float fadeTime = 1.0f;
            float timer = 0f;
            while (timer < fadeTime)
            {
                timer += Time.deltaTime;
                float alpha = 1f - (timer / fadeTime);
                
                foreach (Material material in fadeMaterials)
                {
                    // Set rendering mode to fade
                    material.SetFloat("_Mode", 2);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    
                    Color color = material.color;
                    material.color = new Color(color.r, color.g, color.b, alpha);
                }
                
                yield return null;
            }
            
            // Make fully invisible before teleporting
            foreach (Material material in fadeMaterials)
            {
                Color color = material.color;
                material.color = new Color(color.r, color.g, color.b, 0);
            }
            
            // Restore original materials later
            yield return new WaitForSeconds(0.5f);
            renderer.materials = originalMaterials;
        }
        
        // Determine teleport position
        Vector3 newPosition;
        
        if (teleportDestination != null)
        {
            // Use the assigned destination
            newPosition = teleportDestination.position;
        }
        else
        {
            // Find a valid random position
            bool validPositionFound = false;
            int maxAttempts = 30;
            newPosition = transform.position; // Default fallback
            
            for (int attempt = 0; attempt < maxAttempts && !validPositionFound; attempt++)
            {
                // Get random direction in a circle
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minTeleportDistanceFromPlayer, teleportRadius);
                Vector3 randomDirection = new Vector3(randomCircle.x, 0, randomCircle.y);
                
                // Calculate potential new position
                Vector3 potentialPosition = playerTransform.position + randomDirection;
                
                // Find ground position
                RaycastHit hit;
                if (Physics.Raycast(potentialPosition + Vector3.up * 10, Vector3.down, out hit, 20f))
                {
                    // Check if position is valid (not in a safe zone, etc.)
                    if (!checkSafe.IsPositionInSafeZone(hit.point))
                    {
                        newPosition = hit.point + Vector3.up * 0.1f; // Slightly above ground
                        validPositionFound = true;
                    }
                }
            }
        }
        
        // Actually teleport the enemy
        transform.position = newPosition;
        
        // Reset the enemy AI state if needed
        if (enemyAiComponent != null)
        {
            enemyAiComponent.ResetState();
            // Tell the AI this is its new spawn position
            enemyAiComponent.SetNewRoamTarget();
            // Restore original speeds
            RestoreOriginalSpeeds();
        }
        
        // Wait a moment before allowing another teleport
        yield return new WaitForSeconds(1f);
        isTeleporting = false;
    }

    // Handle cooldown logic and visual feedback
    private void HandleRotationCooldown()
    {
        if (isRotationOnCooldown)
        {
            float timeSinceLastRotation = Time.time - lastRotationTime;
            
            if (timeSinceLastRotation >= rotationCooldown)
            {
                // Cooldown complete
                isRotationOnCooldown = false;
                
                // Hide cooldown indicator when done
                if (cooldownIndicator != null)
                {
                    cooldownIndicator.gameObject.SetActive(false);
                }
            }
            else
            {
                // Update cooldown indicator if available
                if (cooldownIndicator != null)
                {
                    // Calculate remaining cooldown percentage (decreasing from 1 to 0)
                    float cooldownRemaining = 1 - (timeSinceLastRotation / rotationCooldown);
                    cooldownIndicator.fillAmount = cooldownRemaining;
                }
            }
        }
    }

    void TriggerEnemyRotation()
    {
        // Find all enemies with enemyAi component
        enemyAi[] enemies;

        if (!string.IsNullOrEmpty(enemyTag))
        {
            // Find enemies with the specified tag
            GameObject[] taggedEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
            List<enemyAi> enemyList = new List<enemyAi>();

            foreach (GameObject enemy in taggedEnemies)
            {
                enemyAi ai = enemy.GetComponent<enemyAi>();
                if (ai != null)
                {
                    enemyList.Add(ai);
                }
            }

            enemies = enemyList.ToArray();
        }
        else
        {
            // Find all enemyAi components
            enemies = FindObjectsOfType<enemyAi>();
        }

        // Calculate player forward direction
        Vector3 playerForward = transform.forward;
        Vector3 playerPosition = transform.position;

        foreach (enemyAi enemy in enemies)
        {
            // Skip if null
            if (enemy == null) continue;

            // Check distance
            float distance = Vector3.Distance(playerPosition, enemy.transform.position);

            if (distance <= maxAffectDistance)
            {
                // Stop any existing rotation for this enemy
                if (activeRotations.ContainsKey(enemy) && activeRotations[enemy] != null)
                {
                    StopCoroutine(activeRotations[enemy]);
                }

                // Start new rotation coroutine
                Coroutine rotationCoroutine = StartCoroutine(RotateEnemyOverTime(enemy, playerForward));
                activeRotations[enemy] = rotationCoroutine;
            }
        }
    }

    IEnumerator RotateEnemyOverTime(enemyAi enemy, Vector3 direction)
    {
        // Mark this enemy as being rotated
        activeRotatingEnemies.Add(enemy);

        // Calculate rotation away from player
        Vector3 awayDirection = -direction;
        Quaternion targetRotation = Quaternion.LookRotation(awayDirection);

        float elapsedTime = 0f;

        // Get the starting rotation
        Quaternion startRotation = enemy.transform.rotation;

        // Rotate over time
        while (elapsedTime < effectDuration)
        {
            // If enemy was destroyed during rotation
            if (enemy == null)
                yield break;

            // Calculate rotation progress
            float t = elapsedTime / effectDuration;
            enemy.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final rotation is applied
        if (enemy != null)
        {
            enemy.transform.rotation = targetRotation;
            activeRotatingEnemies.Remove(enemy);
        }
    }

    // Method to freeze or unfreeze the enemy when illuminated by flashlight
    public void FreezeEnemy(bool freeze)
    {
        // Only process if the freeze state is changing
        if (isFrozen == freeze || !canBeFrozenByFlashlight)
            return;

        isFrozen = freeze;

        if (enemyAiComponent != null)
        {
            if (freeze)
            {
                // Store original speeds and stop movement
                StoreOriginalSpeeds();
                SetAllSpeeds(0f);

                // Spawn visual effect if provided
                if (frozenEffectPrefab != null && activeFrozenEffect == null)
                {
                    activeFrozenEffect = Instantiate(frozenEffectPrefab, transform.position, Quaternion.identity);
                    activeFrozenEffect.transform.SetParent(transform);
                }
            }
            else
            {
                // Restore original movement speeds
                RestoreOriginalSpeeds();

                // Remove visual effect if it exists
                if (activeFrozenEffect != null)
                {
                    Destroy(activeFrozenEffect);
                    activeFrozenEffect = null;
                }
            }
        }
    }

    // Helper method to store all original speed values
    private void StoreOriginalSpeeds()
    {
        originalNormalSpeed = enemyAiComponent.normalSpeed;
        originalChaseSpeed = enemyAiComponent.chaseSpeed;
        originalSlowSpeed = enemyAiComponent.slowSpeed;
        originalRoamSpeed = enemyAiComponent.roamSpeed;
    }

    // Helper method to set all speeds to the same value
    private void SetAllSpeeds(float speed)
    {
        enemyAiComponent.normalSpeed = speed;
        enemyAiComponent.chaseSpeed = speed;
        enemyAiComponent.slowSpeed = speed;
        enemyAiComponent.roamSpeed = speed;
    }

    // Helper method to restore original speeds
    private void RestoreOriginalSpeeds()
    {
        enemyAiComponent.normalSpeed = originalNormalSpeed;
        enemyAiComponent.chaseSpeed = originalChaseSpeed;
        enemyAiComponent.slowSpeed = originalSlowSpeed;
        enemyAiComponent.roamSpeed = originalRoamSpeed;
    }

    // Public method to check if enemy is frozen
    public bool IsFrozen()
    {
        return isFrozen;
    }
    
    // Public method to check if rotation ability is on cooldown
    public bool IsRotationOnCooldown()
    {
        return isRotationOnCooldown;
    }
    
    // Public method to get cooldown progress (0-1, where 0 means ready)
    public float GetRotationCooldownProgress()
    {
        if (!isRotationOnCooldown)
            return 0;
            
        float timeSinceLastRotation = Time.time - lastRotationTime;
        return 1 - Mathf.Clamp01(timeSinceLastRotation / rotationCooldown);
    }
    
    // Public method to check if enemy is currently being teleported
    public bool IsTeleporting()
    {
        return isTeleporting;
    }
}
