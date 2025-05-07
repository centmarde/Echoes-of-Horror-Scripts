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

    // Internal variables
    private List<enemyAi> activeRotatingEnemies = new List<enemyAi>();
    private Dictionary<enemyAi, Coroutine> activeRotations = new Dictionary<enemyAi, Coroutine>();

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
}
