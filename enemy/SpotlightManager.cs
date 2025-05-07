using UnityEngine;

public class SpotlightManager : MonoBehaviour
{
    [Header("Spotlight Settings")]
    [Tooltip("Automatically turn on spotlight when enemy catches the player")]
    public bool enableSpotlightOnCatch = true;
    
    [Tooltip("Prefab for spotlight (optional - will create one if not assigned)")]
    public Light spotlightPrefab;
    
    [Tooltip("Color of the spotlight")]
    [HideInInspector]
    public Color spotlightColor = Color.red;
    
    [Tooltip("Intensity of the spotlight")]
    [HideInInspector]
    [Range(1f, 10f)]
    public float spotlightIntensity = 5f;
    
    [Tooltip("Range of the spotlight")]
    [HideInInspector]
    public float spotlightRange = 10f;
    
    [Tooltip("Spot angle of the spotlight")]
    [HideInInspector]
    [Range(1f, 179f)]
    public float spotlightAngle = 30f;
    
    [Tooltip("Y offset above the monster")]
    [HideInInspector]
    public float heightOffset = 7f;
    
    private Light currentSpotlight;
    private bool isSpotlightActive = false;
    
    // Creates a spotlight at the given position or under the specified transform
    public Light CreateSpotlight(Transform targetTransform)
    {
        // If a spotlight already exists, use that one
        if (currentSpotlight != null)
        {
            // Position the existing spotlight
            PositionSpotlightAboveTarget(currentSpotlight, targetTransform);
            
            // Make sure it's turned on
            if (!currentSpotlight.gameObject.activeSelf)
            {
                currentSpotlight.gameObject.SetActive(true);
            }
            
            isSpotlightActive = true;
            return currentSpotlight;
        }
        
        // Create a new spotlight
        GameObject spotlightObject;
        
        // Use prefab if available, otherwise create a new one
        if (spotlightPrefab != null)
        {
            spotlightObject = Instantiate(spotlightPrefab.gameObject);
            currentSpotlight = spotlightObject.GetComponent<Light>();
        }
        else
        {
            spotlightObject = new GameObject("CatchSequenceSpotlight");
            currentSpotlight = spotlightObject.AddComponent<Light>();
            
            // Configure the spotlight
            currentSpotlight.type = LightType.Spot;
            currentSpotlight.color = spotlightColor;
            currentSpotlight.intensity = spotlightIntensity;
            currentSpotlight.range = spotlightRange;
            currentSpotlight.spotAngle = spotlightAngle;
            
            // Add some shadows for dramatic effect
            currentSpotlight.shadows = LightShadows.Hard;
        }
        
        // Position the spotlight above the target
        PositionSpotlightAboveTarget(currentSpotlight, targetTransform);
        
        isSpotlightActive = true;
        return currentSpotlight;
    }
    
    // Toggle the spotlight on or off
    public void ToggleSpotlight(bool turnOn)
    {
        if (currentSpotlight != null)
        {
            currentSpotlight.gameObject.SetActive(turnOn);
            isSpotlightActive = turnOn;
        }
        else if (turnOn)
        {
            // If trying to turn on but no spotlight exists, create a default one
            GameObject spotlightObject = new GameObject("CatchSequenceSpotlight");
            currentSpotlight = spotlightObject.AddComponent<Light>();
            currentSpotlight.type = LightType.Spot;
            currentSpotlight.color = spotlightColor;
            currentSpotlight.intensity = spotlightIntensity;
            currentSpotlight.range = spotlightRange;
            currentSpotlight.spotAngle = spotlightAngle;
            currentSpotlight.shadows = LightShadows.Hard;
            
            // Position above the center of the scene (this is a fallback)
            spotlightObject.transform.position = new Vector3(0, heightOffset, 0);
            spotlightObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            
            isSpotlightActive = true;
        }
    }
    
    // Remove the spotlight by destroying the game object
    public void RemoveSpotlight()
    {
        if (currentSpotlight != null)
        {
            Destroy(currentSpotlight.gameObject);
            currentSpotlight = null;
            isSpotlightActive = false;
        }
    }
    
    // Check if spotlight is active
    public bool IsSpotlightActive()
    {
        return isSpotlightActive;
    }
    
    // Position the spotlight above the target looking down
    private void PositionSpotlightAboveTarget(Light spotlight, Transform targetTransform)
    {
        Vector3 spotlightPosition = targetTransform.position + Vector3.up * heightOffset;
        spotlight.transform.position = spotlightPosition;
        
        // Point the spotlight directly downward
        spotlight.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
