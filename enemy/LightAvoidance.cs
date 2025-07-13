using System.Collections;
using UnityEngine;

[RequireComponent(typeof(enemyAi))]
public class LightAvoidance : MonoBehaviour
{
    [Header("Light Avoidance Settings")]
    [Tooltip("Enable/disable light avoidance functionality")]
    public bool enableLightAvoidance = true;
    
    [Tooltip("Radius to detect light sources around the enemy")]
    public float lightDetectionRadius = 2f;
    
    [Tooltip("Time delay before teleporting after touching light")]
    public float teleportDelay = 0.1f;
    
    [Tooltip("Effect duration for disappear/reappear visual")]
    public float disappearEffectDuration = 0.5f;
    
    [Header("Debug Light Avoidance")]
    [SerializeField] private bool showLightDebugInfo = true;
    [SerializeField] private bool isInLightContact = false;
    [SerializeField] private int lightSourcesDetected = 0;
    [SerializeField] private string lastLightSourceName = "";
    
    // Private variables for light avoidance
    private bool isTeleporting = false;
    private Renderer enemyRenderer;
    private Collider enemyCollider;
    private Vector3 spawnPosition;
    private enemyAi enemyAIComponent;
    
    private void Start()
    {
        // Get the enemyAi component
        enemyAIComponent = GetComponent<enemyAi>();
        if (enemyAIComponent == null)
        {
            Debug.LogError("LightAvoidance: No enemyAi component found! This script requires enemyAi to function.");
            enabled = false;
            return;
        }
        
        // Store spawn position from the enemy's current position
        spawnPosition = transform.position;
        
        // Get renderer and collider components
        enemyRenderer = GetComponent<Renderer>();
        enemyCollider = GetComponent<Collider>();
        
        if (enemyRenderer == null)
        {
            // Try to find renderer in children if not on main object
            enemyRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (enemyCollider == null)
        {
            Debug.LogWarning("LightAvoidance: No Collider component found. Light detection may not work properly.");
        }
        
        if (showLightDebugInfo)
        {
            Debug.Log($"LightAvoidance initialized. Spawn position: {spawnPosition}");
        }
    }
    
    private void Update()
    {
        // Only check for lights if not already teleporting, light avoidance is enabled, and enemyAI exists
        if (!isTeleporting && enableLightAvoidance && enemyAIComponent != null)
        {
            CheckForNearbyLights();
        }
    }
    
    private void CheckForNearbyLights()
    {
        // Find all lights within detection radius
        Light[] nearbyLights = FindObjectsOfType<Light>();
        lightSourcesDetected = 0;
        isInLightContact = false;
        
        foreach (Light light in nearbyLights)
        {
            // Check if light has the correct tag
            if (light.CompareTag("light"))
            {
                float distanceToLight = Vector3.Distance(transform.position, light.transform.position);
                
                // Check if enemy is within the light's range or our detection radius
                float checkRadius = Mathf.Max(lightDetectionRadius, light.range);
                
                if (distanceToLight <= checkRadius)
                {
                    lightSourcesDetected++;
                    isInLightContact = true;
                    lastLightSourceName = light.gameObject.name;
                    
                    if (showLightDebugInfo)
                    {
                        Debug.Log($"Enemy detected light '{light.gameObject.name}' at distance {distanceToLight:F2}. Light range: {light.range:F2}");
                    }
                    
                    // Trigger teleport
                    StartCoroutine(TeleportToSpawn(light.gameObject.name));
                    return; // Exit early since we're teleporting
                }
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!enableLightAvoidance || isTeleporting) return;
        
        // Check if the collided object has a Light component and the correct tag
        Light lightComponent = other.GetComponent<Light>();
        if (lightComponent != null && other.CompareTag("ligt"))
        {
            isInLightContact = true;
            lastLightSourceName = other.gameObject.name;
            
            if (showLightDebugInfo)
            {
                Debug.Log($"Enemy touched light source '{other.gameObject.name}' via trigger. Initiating teleport.");
            }
            
            StartCoroutine(TeleportToSpawn(other.gameObject.name));
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (!enableLightAvoidance || isTeleporting) return;
        
        // Check if the collided object has a Light component and the correct tag
        Light lightComponent = collision.gameObject.GetComponent<Light>();
        if (lightComponent != null && collision.gameObject.CompareTag("ligt"))
        {
            isInLightContact = true;
            lastLightSourceName = collision.gameObject.name;
            
            if (showLightDebugInfo)
            {
                Debug.Log($"Enemy collided with light source '{collision.gameObject.name}'. Initiating teleport.");
            }
            
            StartCoroutine(TeleportToSpawn(collision.gameObject.name));
        }
    }
    
    private IEnumerator TeleportToSpawn(string lightSourceName)
    {
        if (isTeleporting) yield break; // Prevent multiple teleports
        
        isTeleporting = true;
        
        if (showLightDebugInfo)
        {
            Debug.Log($"Starting teleport sequence due to light '{lightSourceName}'. Current position: {transform.position}, Target: {spawnPosition}");
        }
        
        // Disable AI behavior during teleport by setting catch sequence active
        if (enemyAIComponent != null)
        {
            enemyAIComponent.SetCatchSequenceActive(true);
        }
        
        // Optional: Add disappear effect
        yield return StartCoroutine(DisappearEffect());
        
        // Wait for teleport delay
        yield return new WaitForSeconds(teleportDelay);
        
        // Teleport to spawn position
        transform.position = spawnPosition;
        
        if (showLightDebugInfo)
        {
            Debug.Log($"Enemy teleported to spawn position: {spawnPosition}");
        }
        
        // Optional: Add reappear effect
        yield return StartCoroutine(ReappearEffect());
        
        // Reset AI state
        if (enemyAIComponent != null)
        {
            enemyAIComponent.ResetState();
            enemyAIComponent.SetCatchSequenceActive(false);
        }
        
        // Reset teleport flag
        isTeleporting = false;
        isInLightContact = false;
        
        if (showLightDebugInfo)
        {
            Debug.Log("Teleport sequence completed. AI state reset.");
        }
    }
    
    private IEnumerator DisappearEffect()
    {
        if (enemyRenderer == null) yield break;
        
        float elapsedTime = 0f;
        Color originalColor = enemyRenderer.material.color;
        
        // Fade out
        while (elapsedTime < disappearEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / disappearEffectDuration);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            enemyRenderer.material.color = newColor;
            
            yield return null;
        }
        
        // Ensure completely invisible
        Color invisibleColor = originalColor;
        invisibleColor.a = 0f;
        enemyRenderer.material.color = invisibleColor;
        
        // Optionally disable collider during teleport
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }
    
    private IEnumerator ReappearEffect()
    {
        if (enemyRenderer == null) yield break;
        
        float elapsedTime = 0f;
        Color originalColor = enemyRenderer.material.color;
        originalColor.a = 1f; // Ensure we know the target alpha
        
        // Re-enable collider
        if (enemyCollider != null)
        {
            enemyCollider.enabled = true;
        }
        
        // Fade in
        while (elapsedTime < disappearEffectDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / disappearEffectDuration);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            enemyRenderer.material.color = newColor;
            
            yield return null;
        }
        
        // Ensure completely visible
        enemyRenderer.material.color = originalColor;
    }
    
    // Public method to manually set spawn position if needed
    public void SetSpawnPosition(Vector3 newSpawnPosition)
    {
        spawnPosition = newSpawnPosition;
        
        if (showLightDebugInfo)
        {
            Debug.Log($"Spawn position updated to: {spawnPosition}");
        }
    }
    
    // Public method to get current spawn position
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition;
    }
    
    // Public method to manually trigger teleport (for testing)
    public void ManualTeleportToSpawn()
    {
        if (!isTeleporting)
        {
            StartCoroutine(TeleportToSpawn("Manual Trigger"));
        }
    }
    
    // Public method to check if currently teleporting (useful for other scripts)
    public bool IsTeleporting()
    {
        return isTeleporting;
    }
    
    // Visualize the light detection in the editor
    private void OnDrawGizmosSelected()
    {
        if (!showLightDebugInfo) return;
        
        // Draw light detection radius
        Gizmos.color = isInLightContact ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightDetectionRadius);
        
        // Draw spawn position
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition, 0.5f);
        Gizmos.DrawLine(transform.position, spawnPosition);
        
        // Draw connections to nearby lights with correct tag
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            if (light.CompareTag("ligt"))
            {
                float distance = Vector3.Distance(transform.position, light.transform.position);
                float checkRadius = Mathf.Max(lightDetectionRadius, light.range);
                
                if (distance <= checkRadius)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, light.transform.position);
                    
                    // Draw light range
                    Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                    Gizmos.DrawSphere(light.transform.position, light.range);
                }
            }
        }
    }
}
