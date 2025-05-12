using UnityEngine;

public class PlayerSpawnManager : MonoBehaviour
{
    // Singleton instance for easy access
    public static PlayerSpawnManager Instance { get; private set; }

    // Store original spawn position and rotation
    private Vector3 originalSpawnPosition;
    private Quaternion originalSpawnRotation;
    
    // Flag to track if the position has been set
    private bool hasSpawnPointBeenSet = false;
    
    [Tooltip("Optional manually assigned spawn point. If not set, player's starting position is used.")]
    public Transform manualSpawnPoint;

    private void Awake()
    {
        // Singleton pattern setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // If a manual spawn point is set, use that immediately
        if (manualSpawnPoint != null)
        {
            originalSpawnPosition = manualSpawnPoint.position;
            originalSpawnRotation = manualSpawnPoint.rotation;
            hasSpawnPointBeenSet = true;
            Debug.Log($"PlayerSpawnManager: Using manual spawn point at {originalSpawnPosition}");
        }
    }
    
    private void Start()
    {
        // If no manual spawn point was set, try to find the player and use its position
        if (!hasSpawnPointBeenSet)
        {
            SetSpawnPointFromPlayer();
        }
    }
    
    // Call this method to set the spawn point from the current player position
    public void SetSpawnPointFromPlayer()
    {
        // Find player using various methods
        Transform playerTransform = FindPlayerTransform();
        
        if (playerTransform != null)
        {
            originalSpawnPosition = playerTransform.position;
            originalSpawnRotation = playerTransform.rotation;
            hasSpawnPointBeenSet = true;
            Debug.Log($"PlayerSpawnManager: Set spawn point at {originalSpawnPosition}");
        }
        else
        {
            Debug.LogWarning("PlayerSpawnManager: Could not find player to set spawn point");
        }
    }
    
    // Helper method to find the player transform
    private Transform FindPlayerTransform()
    {
        // Try finding by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            return playerObj.transform;
        }
        
        // Try finding by component
        FirstPersonController fpc = FindObjectOfType<FirstPersonController>();
        if (fpc != null)
        {
            return fpc.transform;
        }
        
        return null;
    }
    
    // Public getter methods
    public Vector3 GetSpawnPosition()
    {
        // If position hasn't been set yet, try setting it now
        if (!hasSpawnPointBeenSet)
        {
            SetSpawnPointFromPlayer();
        }
        
        return originalSpawnPosition;
    }
    
    public Quaternion GetSpawnRotation()
    {
        return originalSpawnRotation;
    }
    
    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (hasSpawnPointBeenSet)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(originalSpawnPosition, 0.5f);
            
            // Draw direction
            Gizmos.color = Color.blue;
            Vector3 direction = originalSpawnRotation * Vector3.forward;
            Gizmos.DrawRay(originalSpawnPosition, direction * 2f);
        }
    }
}
