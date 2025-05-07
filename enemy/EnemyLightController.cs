using UnityEngine;

[RequireComponent(typeof(Light))]
public class EnemyLightController : MonoBehaviour
{
    [Header("Light Settings")]
    [Tooltip("Should the light be on when the game starts")]
    public bool startEnabled = false;
    
    [Tooltip("Automatically turn on light when catching player")]
    public bool enableOnCatch = true;

    private Light enemyLight;

    private void Awake()
    {
        // Get the attached light component
        enemyLight = GetComponent<Light>();
        
        // Set initial state
        enemyLight.enabled = startEnabled;
    }

    /// <summary>
    /// Turns the light on or off
    /// </summary>
    public void ToggleLight(bool turnOn)
    {
        if (enemyLight != null)
        {
            enemyLight.enabled = turnOn;
        }
    }

    /// <summary>
    /// Toggles the light from current state to the opposite state
    /// </summary>
    public void ToggleLight()
    {
        if (enemyLight != null)
        {
            enemyLight.enabled = !enemyLight.enabled;
        }
    }
    
    /// <summary>
    /// Check if the light is currently on
    /// </summary>
    public bool IsLightOn()
    {
        return enemyLight != null && enemyLight.enabled;
    }
}
