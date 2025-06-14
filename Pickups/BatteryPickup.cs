using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [Header("Battery Settings")]
    public float batteryAmount = 30f;  // How much battery this pickup provides
    
    [Header("Pickup Settings")]
    public bool destroyOnPickup = true;
    public AudioClip pickupSound;
    public float pickupSoundVolume = 1.0f;
    
    [Header("Visual Effects")]
    public bool enableGlow = true;
    public float glowIntensity = 1.0f;
    public Color glowColor = Color.yellow;
    
    private void Start()
    {
        if(enableGlow)
        {
            // Add a light component for glow effect
            Light glowLight = gameObject.AddComponent<Light>();
            glowLight.color = glowColor;
            glowLight.intensity = glowIntensity;
            glowLight.range = 2f;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            // Find the flashlight script on the player or its children
            flashlight playerFlashlight = other.GetComponentInChildren<flashlight>();
            
            if (playerFlashlight != null)
            {
                // Add battery to the flashlight
                playerFlashlight.AddBattery(batteryAmount);
                
                // Play pickup sound if available
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
                }
                
                // Destroy the battery pickup if set to do so
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    // Optional: Add a rotating effect to make the battery more noticeable
    void Update()
    {
        transform.Rotate(Vector3.up, 50f * Time.deltaTime);
    }
}
