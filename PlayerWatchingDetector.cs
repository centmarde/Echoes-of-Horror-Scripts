using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWatchingDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("The camera to use for watching detection. Will auto-find if not set.")]
    public Camera playerCamera;
    
    [Tooltip("Field of view angle for detecting when player is looking at enemy")]
    [Range(5f, 60f)]
    public float detectionAngle = 30f;
    
    [Tooltip("Maximum distance for watching detection")]
    public float maxDetectionDistance = 20f;
    
    [Tooltip("Should line of sight be required for detection?")]
    public bool requireLineOfSight = true;
    
    [Tooltip("Layer mask for line of sight obstruction")]
    public LayerMask obstructionMask = -1;
    
    [Tooltip("How long player must look at enemy before it freezes")]
    public float watchTimeThreshold = 0.1f;
      [Tooltip("Layer mask for enemy objects")]
    public LayerMask enemyLayerMask = -1;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private GameObject currentWatchedEnemy = null;
    [SerializeField] private float currentWatchTime = 0f;
    
    // Private variables
    private Dictionary<GameObject, float> enemyWatchTimes = new Dictionary<GameObject, float>();
    private FirstPersonController playerController;
    
    private void Start()
    {
        // Find the player camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }
            if (playerCamera == null)
            {
                Debug.LogWarning("PlayerWatchingDetector: No camera found! Please assign manually.");
            }
        }
        
        // Get the FirstPersonController component
        playerController = GetComponent<FirstPersonController>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerWatchingDetector: No FirstPersonController found. Some features may not work correctly.");
        }
    }
    
    private void Update()
    {
        if (playerCamera == null) return;
        
        // Update watching detection
        UpdateWatchingDetection();
        
        // Clean up watch times for destroyed objects
        CleanupWatchTimes();
    }
    
    private void UpdateWatchingDetection()
    {
        GameObject previousWatchedEnemy = currentWatchedEnemy;
        currentWatchedEnemy = null;
        currentWatchTime = 0f;
        
        // Cast a ray from the camera center
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxDetectionDistance))
        {
            // Check if we hit an enemy
            GameObject hitObject = hit.collider.gameObject;
            
            // Check if the hit object has an enemy AI component or is on the enemy layer
            bool isEnemy = hitObject.GetComponent<enemyAi>() != null || 
                          hitObject.GetComponent<enemyAi2>() != null ||
                          (enemyLayerMask == (enemyLayerMask | (1 << hitObject.layer)));
            
            if (isEnemy)
            {
                // Check if the enemy is within the detection angle
                Vector3 directionToEnemy = (hit.point - playerCamera.transform.position).normalized;
                float angleToEnemy = Vector3.Angle(playerCamera.transform.forward, directionToEnemy);
                
                if (angleToEnemy <= detectionAngle * 0.5f)
                {
                    // Check line of sight if required
                    bool hasLineOfSight = true;
                    if (requireLineOfSight)
                    {
                        RaycastHit sightHit;
                        if (Physics.Raycast(playerCamera.transform.position, directionToEnemy, out sightHit, hit.distance, obstructionMask))
                        {
                            // If we hit something other than the enemy, line of sight is blocked
                            if (sightHit.collider.gameObject != hitObject)
                            {
                                hasLineOfSight = false;
                            }
                        }
                    }
                    
                    if (hasLineOfSight)
                    {
                        currentWatchedEnemy = hitObject;
                        
                        // Update watch time for this enemy
                        if (!enemyWatchTimes.ContainsKey(hitObject))
                        {
                            enemyWatchTimes[hitObject] = 0f;
                        }
                        
                        enemyWatchTimes[hitObject] += Time.deltaTime;
                        currentWatchTime = enemyWatchTimes[hitObject];
                        
                        if (showDebugInfo)
                        {
                            Debug.Log($"Watching enemy: {hitObject.name}, Time: {currentWatchTime:F2}");
                        }
                    }
                }
            }
        }
        
        // Reset watch times for enemies we're no longer looking at
        List<GameObject> enemiesToReset = new List<GameObject>();
        foreach (var kvp in enemyWatchTimes)
        {
            if (kvp.Key != currentWatchedEnemy)
            {
                enemiesToReset.Add(kvp.Key);
            }
        }
        
        foreach (var enemy in enemiesToReset)
        {
            enemyWatchTimes[enemy] = 0f;
        }
    }
    
    private void CleanupWatchTimes()
    {
        // Remove entries for destroyed GameObjects
        List<GameObject> toRemove = new List<GameObject>();
        foreach (var kvp in enemyWatchTimes)
        {
            if (kvp.Key == null)
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        foreach (var key in toRemove)
        {
            enemyWatchTimes.Remove(key);
        }
    }
    
    // Public method for enemies to check if they're being watched
    public bool IsWatchingEnemy(GameObject enemy)
    {
        if (enemy == null) return false;
        
        // Check if this enemy is currently being watched and for long enough
        if (currentWatchedEnemy == enemy && enemyWatchTimes.ContainsKey(enemy))
        {
            return enemyWatchTimes[enemy] >= watchTimeThreshold;
        }
        
        return false;
    }
    
    // Public method to get how long an enemy has been watched
    public float GetWatchTimeForEnemy(GameObject enemy)
    {
        if (enemy == null || !enemyWatchTimes.ContainsKey(enemy))
            return 0f;
            
        return enemyWatchTimes[enemy];
    }
    
    // Public method to reset watch time for a specific enemy
    public void ResetWatchTimeForEnemy(GameObject enemy)
    {
        if (enemy != null && enemyWatchTimes.ContainsKey(enemy))
        {
            enemyWatchTimes[enemy] = 0f;
        }
    }
    
    // Method to get the currently watched enemy
    public GameObject GetCurrentWatchedEnemy()
    {
        return currentWatchedEnemy;
    }
      // Method to check if any enemy is being watched
    public bool IsWatchingAnyEnemy()
    {
        return currentWatchedEnemy != null && currentWatchTime >= watchTimeThreshold;
    }
      // Debug visualization - disabled for invisible system
    private void OnDrawGizmos()
    {
        // All gizmo drawing has been removed for invisible operation
        return;
    }
}
