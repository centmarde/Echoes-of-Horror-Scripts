using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkSafe : MonoBehaviour
{
    private static List<Collider> safeZones = new List<Collider>();
    private enemyAi enemyAiComponent;
    
    // References needed for teleporting
    private Rigidbody enemyRigidbody;
    private Vector3 spawnPosition;

    private void Awake()
    {
        enemyAiComponent = GetComponent<enemyAi>();
        enemyRigidbody = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        // Store the spawn position at start
        spawnPosition = transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the enemy entered a safe zone (note: the enemy has this script)
        if (collision.gameObject.CompareTag("SafeZone") && enemyAiComponent != null)
        {
            Debug.Log("Enemy entered safe zone - teleporting away");
            TeleportToSpawnPosition();
        }
        Debug.Log($"Enemy {gameObject.name} entered safe zone: {collision.gameObject.name}");
    }

    // Check if a position is within any safe zone
    public static bool IsPositionInSafeZone(Vector3 position)
    {
        foreach (Collider safeZone in safeZones)
        {
            if (safeZone != null && safeZone.bounds.Contains(position))
            {
                return true;
            }
        }
        return false;
    }

    // Check if the path between two points crosses through a safe zone
    public static bool IsPathCrossingSafeZone(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        
        // Check multiple points along the path
        int checkPoints = Mathf.Max(5, Mathf.FloorToInt(distance));
        for (int i = 0; i <= checkPoints; i++)
        {
            float t = i / (float)checkPoints;
            Vector3 pointOnPath = Vector3.Lerp(start, end, t);
            if (IsPositionInSafeZone(pointOnPath))
            {
                return true;
            }
        }
        
        return false;
    }

    // Find closest position outside any safe zone
    public static Vector3 GetNearestPointOutsideSafeZone(Vector3 currentPosition)
    {
        // If not in a safe zone, return current position
        if (!IsPositionInSafeZone(currentPosition))
        {
            return currentPosition;
        }
        
        // Try different directions to escape the safe zone
        float escapeRadius = 5.0f;
        Vector3[] directions = new Vector3[] {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right,
            (Vector3.forward + Vector3.right).normalized,
            (Vector3.forward + Vector3.left).normalized,
            (Vector3.back + Vector3.right).normalized,
            (Vector3.back + Vector3.left).normalized
        };
        
        foreach (Vector3 dir in directions)
        {
            Vector3 potentialPosition = currentPosition + dir * escapeRadius;
            if (!IsPositionInSafeZone(potentialPosition))
            {
                return potentialPosition;
            }
        }
        
        // If all else fails, try a larger radius
        return currentPosition + directions[0] * escapeRadius * 2;
    }

    // Get a safe position to navigate to
    public static Vector3 GetNearestSafePosition(Vector3 targetPosition, Vector3 currentPosition)
    {
        if (!IsPositionInSafeZone(targetPosition))
        {
            return targetPosition;
        }
        
        // Try to find a position outside the safe zone in the direction from current to target
        Vector3 direction = (targetPosition - currentPosition).normalized;
        float searchDistance = 10f;
        
        // Try different distances
        for (int i = 2; i <= 5; i++)
        {
            Vector3 potentialPosition = targetPosition + direction * (searchDistance * i);
            if (!IsPositionInSafeZone(potentialPosition))
            {
                return potentialPosition;
            }
        }
        
        // If direction search fails, try nearby options
        Vector3[] alternateDirections = new Vector3[] {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right
        };
        
        foreach (Vector3 dir in alternateDirections)
        {
            Vector3 potentialPosition = targetPosition + dir * searchDistance;
            if (!IsPositionInSafeZone(potentialPosition))
            {
                return potentialPosition;
            }
        }
        
        return currentPosition; // If all else fails, stay put
    }
    
    // Reset to idle state when entering a safe zone
    public void CancelChase()
    {
        if (enemyAiComponent != null)
        {
            enemyAiComponent.CancelChase();
        }
        else
        {
            Debug.LogError("Cannot cancel chase: enemyAi component not found!");
        }
    }

    // Method to teleport enemy back to spawn position when crossing safe zone
    public void TeleportToSpawnPosition()
    {
        StartCoroutine(DisappearAndTeleport());
    }
    
    private IEnumerator DisappearAndTeleport()
    {
        // Make the enemy invisible
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.enabled = false;
        }
        
        // Disable colliders to prevent further interactions
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
        {
            if (c != GetComponent<Collider>()) // Keep the main collider active
            {
                c.enabled = false;
            }
        }
        
        // Cancel chase behavior
        CancelChase();
        
        // Wait for a moment
        yield return new WaitForSeconds(1.5f);
        
        // Reset position to spawn point
        if (enemyRigidbody != null)
        {
            enemyRigidbody.velocity = Vector3.zero;
            enemyRigidbody.position = spawnPosition;
        }
        else
        {
            transform.position = spawnPosition;
        }
        
        // Set a new roam target - call through the enemyAi component
        if (enemyAiComponent != null)
        {
            enemyAiComponent.SetNewRoamTarget();
        }
        
        // Wait an additional moment before reappearing
        yield return new WaitForSeconds(0.5f);
        
        // Make the enemy visible again
        foreach (Renderer r in renderers)
        {
            r.enabled = true;
        }
        
        // Re-enable colliders
        foreach (Collider c in colliders)
        {
            c.enabled = true;
        }
        
        Debug.Log("Enemy teleported back to spawn position");
    }
}
