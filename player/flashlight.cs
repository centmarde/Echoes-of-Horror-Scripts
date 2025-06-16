using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class flashlight : MonoBehaviour
{
    // Reference to the Light component
    private Light flashlightLight;
    
    // Track if the flashlight is on or off
    private bool isOn = false;
    
    // Flashlight battery settings
    [Header("Battery Settings")]
    public float batteryDuration = 60f;     // How many seconds the battery lasts
    public float batteryRemaining;           // Current battery level
    public float batteryDrainRate = 1f;      // How quickly battery drains when on
    
    [Header("Enemy Detection")]
    [Tooltip("Maximum distance to detect enemies")]
    public float detectionDistance = 20f;
    
    [Tooltip("Cone angle for enemy detection")]
    public float detectionConeAngle = 30f;
    
    [Tooltip("Should enemies teleport when hit by flashlight")]
    public bool enableEnemyTeleport = true;
    
    // Reference to the FirstPersonController for UI
    private FirstPersonController playerController;
    private bool hasInitializedUI = false;
    
    // Add a reference to the detector
    private FlashlightDetector detector;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get reference to the Light component
        flashlightLight = GetComponent<Light>();
        
        // Initialize battery
        batteryRemaining = batteryDuration;
        
        // Find the player controller
        playerController = FindObjectOfType<FirstPersonController>();
        
        // Set initial state
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isOn;
        }
        
        // Setup enemy detection
        SetupDetector();
    }

    // Setup the detector component
    private void SetupDetector()
    {
        detector = GetComponent<FlashlightDetector>();
        
        // Add detector if it doesn't exist
        if (detector == null)
        {
            detector = gameObject.AddComponent<FlashlightDetector>();
            detector.maxDetectionDistance = detectionDistance;
            detector.detectionAngle = detectionConeAngle;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Initialize UI if not done yet
        if (!hasInitializedUI && playerController != null)
        {
            InitializeFlashlightBar();
            hasInitializedUI = true;
        }
        
        // Check for 'R' key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Only allow toggling on if there's battery remaining
            if (!isOn || batteryRemaining > 0)
            {
                ToggleLight();
            }
        }
        
        // Handle battery logic
        HandleBattery();
        
        // Update the flashlight bar if available
        UpdateFlashlightBar();
    }
    
    // Toggle the light on and off
    private void ToggleLight()
    {
        if (flashlightLight != null)
        {
            isOn = !isOn;
            flashlightLight.enabled = isOn;
        }
    }
    
    // Handle battery drain
    private void HandleBattery()
    {
        if (isOn)
        {
            // Drain battery
            batteryRemaining -= batteryDrainRate * Time.deltaTime;
            
            // Turn off if battery is depleted
            if (batteryRemaining <= 0)
            {
                batteryRemaining = 0;
                isOn = false;
                if (flashlightLight != null)
                {
                    flashlightLight.enabled = false;
                }
            }
        }
        // No automatic recharge functionality anymore
    }
    
    // Add battery from pickups
    public void AddBattery(float amount)
    {
        // Add the battery amount
        batteryRemaining = Mathf.Min(batteryRemaining + amount, batteryDuration);
        
        // Update UI immediately
        UpdateFlashlightBar();
    }
    
    // Initialize the flashlight UI bar
    private void InitializeFlashlightBar()
    {
        // ...existing code...
        if (playerController != null && playerController.useFlashlightBar)
        {
            if (playerController.flashlightBarBG != null && playerController.flashlightBar != null)
            {
                // The controller now handles most of the UI setup in Start()
                // We just need to make sure it's visible if needed
                
                // Get flashlight bar canvas group
                CanvasGroup flashlightCG = playerController.GetFlashlightBarCG();
                if (flashlightCG == null)
                {
                    flashlightCG = playerController.flashlightBar.GetComponentInParent<CanvasGroup>();
                    playerController.SetFlashlightBarCG(flashlightCG);
                }
                
                // Hide bar initially if setting is enabled
                if (playerController.GetHideFlashlightBarWhenFull() && flashlightCG != null && batteryRemaining >= batteryDuration)
                {
                    flashlightCG.alpha = 0;
                }
            }
        }
    }
    
       // Update the flashlight UI bar
    private void UpdateFlashlightBar()
    {
        if (playerController != null && playerController.useFlashlightBar)
        {
            if (playerController.flashlightBar != null)
            {
                // Update the flashlight bar scale
                float batteryPercent = batteryRemaining / batteryDuration;
                
                // Keep original scale and adjust width only
                Vector3 currentScale = playerController.flashlightBar.transform.localScale;
                playerController.flashlightBar.transform.localScale = new Vector3(batteryPercent, currentScale.y, currentScale.z);
                
                // Adjust the pivot/anchor point to scale from right to left
                RectTransform rectTransform = playerController.flashlightBar.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Set pivot to right side (1,0.5) if not already set
                   if (rectTransform.pivot.x != 1)
{
    rectTransform.pivot = new Vector2(1, 0.5f);
    // Adjust position to compensate for pivot change if needed
    rectTransform.anchorMin = new Vector2(1, rectTransform.anchorMin.y);
    rectTransform.anchorMax = new Vector2(1, rectTransform.anchorMax.y);
    // Move slightly to the right by adjusting the anchored position
    rectTransform.anchoredPosition = new Vector2(60f, rectTransform.anchoredPosition.y);
}
                }
                
                // Handle bar visibility
                CanvasGroup flashlightCG = playerController.GetFlashlightBarCG();
                if (playerController.hideFlashlightBarWhenFull && flashlightCG != null)
                {
                    if (isOn || batteryRemaining < batteryDuration)
                    {
                        // Show the bar when using flashlight or when not full
                        flashlightCG.alpha = Mathf.Min(flashlightCG.alpha + 3 * Time.deltaTime, 1);
                    }
                    else if (!isOn && batteryRemaining >= batteryDuration)
                    {
                        // Hide the bar when full and not in use
                        flashlightCG.alpha = Mathf.Max(flashlightCG.alpha - 3 * Time.deltaTime, 0);
                    }
                }
            }
        }
    }
    
    // Public method to get if flashlight is on
    public bool IsFlashlightOn()
    {
        return isOn;
    }
    
    // Public method to get battery percentage
    public float GetBatteryPercentage()
    {
        return batteryRemaining / batteryDuration;
    }
}