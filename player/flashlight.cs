using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flashlight : MonoBehaviour
{
    // Reference to the Light component
    private Light flashlightLight;
    
    // Track if the flashlight is on or off
    private bool isOn = false; // Changed from true to false
    
    // Start is called before the first frame update
    void Start()
    {
        // Get reference to the Light component
        flashlightLight = GetComponent<Light>();
        
        // Set initial state
        if (flashlightLight != null)
        {
            flashlightLight.enabled = isOn;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for 'R' key press
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleLight();
        }
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
}
