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

    // Internal variables
    private List<enemyAi> activeRotatingEnemies = new List<enemyAi>();
    private Dictionary<enemyAi, Coroutine> activeRotations = new Dictionary<enemyAi, Coroutine>();

    // Internal variables for frozen state
    private bool isFrozen = false;
    private GameObject activeFrozenEffect = null;
    private enemyAi enemyAiComponent;

    // Original movement speed values to restore after unfreezing
    private float originalNormalSpeed;
    private float originalChaseSpeed;
    private float originalSlowSpeed;
    private float originalRoamSpeed;
    
    // Cooldown tracking variables
    private float lastRotationTime = -1000f; // Initialize to allow immediate first use
    private bool isRotationOnCooldown = false;

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
}
