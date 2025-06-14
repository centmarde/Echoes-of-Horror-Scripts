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

    void Start()
    {
        // Get reference to the enemyAi component
        enemyAiComponent = GetComponent<enemyAi>();

        // Store original movement speeds if component exists
        if (enemyAiComponent != null)
        {
            StoreOriginalSpeeds();
        }
    }

    void Update()
    {
        // Check for key press
        if (Input.GetKeyDown(rotationKey))
        {
            TriggerEnemyRotation();
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
}
