using UnityEngine;

/// <summary>
/// Simple helper component to automatically set up CatchManager for enemies.
/// Attach this to enemy GameObjects along with enemyAi2 and CatchManager components.
/// </summary>
[RequireComponent(typeof(enemyAi2))]
public class EnemyCatchSetup : MonoBehaviour
{
    [Header("Auto Setup")]
    [Tooltip("Should this component automatically find and configure the CatchManager?")]
    public bool autoSetup = true;
    
    [Tooltip("Should we add a trigger collider for catch detection?")]
    public bool addTriggerCollider = true;
    
    [Tooltip("Size of the trigger collider for catch detection")]
    public float triggerSize = 2.5f;
    
    [Header("Manual References")]
    [Tooltip("Manual reference to CatchManager (will auto-find if null)")]
    public CatchManager catchManager;
    
    private void Start()
    {
        if (autoSetup)
        {
            SetupCatchManager();
        }
        
        if (addTriggerCollider)
        {
            SetupTriggerCollider();
        }
    }
    
    private void SetupCatchManager()
    {
        // Get or add CatchManager component
        if (catchManager == null)
        {
            catchManager = GetComponent<CatchManager>();
            if (catchManager == null)
            {
                catchManager = gameObject.AddComponent<CatchManager>();
                Debug.Log($"Added CatchManager component to {gameObject.name}");
            }
        }
        
        // Get enemyAi2 component
        enemyAi2 enemyAI = GetComponent<enemyAi2>();
        if (enemyAI == null)
        {
            Debug.LogError($"EnemyCatchSetup requires enemyAi2 component on {gameObject.name}");
            return;
        }
        
        // Auto-assign enemyAI to CatchManager
        if (catchManager.enemyAI == null)
        {
            catchManager.enemyAI = enemyAI;
            Debug.Log($"Auto-assigned enemyAi2 to CatchManager on {gameObject.name}");
        }
        
        // Auto-find player if not assigned
        if (catchManager.playerController == null)
        {
            FirstPersonController player = FindObjectOfType<FirstPersonController>();
            if (player != null)
            {
                catchManager.playerController = player;
                Debug.Log($"Auto-assigned player to CatchManager on {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"Could not find FirstPersonController for CatchManager on {gameObject.name}");
            }
        }
    }
    
    private void SetupTriggerCollider()
    {
        // Check if there's already a trigger collider
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;
        
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }
        
        if (!hasTrigger)
        {
            // Add a sphere collider as trigger for catch detection
            SphereCollider triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = triggerSize;
            triggerCollider.center = Vector3.zero;
            
            Debug.Log($"Added trigger collider for catch detection on {gameObject.name}");
        }
    }
    
    // Public method to manually trigger setup
    public void ManualSetup()
    {
        SetupCatchManager();
        if (addTriggerCollider)
        {
            SetupTriggerCollider();
        }
    }
    
    // Visualize the trigger area in editor
    private void OnDrawGizmosSelected()
    {
        if (addTriggerCollider)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, triggerSize);
        }
    }
}
