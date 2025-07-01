using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    
    [Header("Floating Effect")]
    public bool enableFloating = true;
    public float floatAmplitude = 0.5f;  // How high/low it floats
    public float floatSpeed = 2f;        // How fast it floats
    
    [Header("Interaction Settings")]
    public KeyCode pickupKey = KeyCode.F;  // Key to press for pickup
    public float interactionRange = 3f;    // Range within which player can pickup
    
    [Header("UI Settings")]
    public GameObject pickupPromptUI;      // UI GameObject to show pickup prompt
    public TextMeshProUGUI promptText;     // Text component for the prompt
    public string promptMessage = "Press \"F\" to pickup";  // Message to display
    
    private Vector3 startPosition;
    private bool playerInRange = false;
    private GameObject currentPlayer;
    
    private void Start()
    {
        // Store the initial position for floating calculation
        startPosition = transform.position;
        if(enableGlow)
        {
            // Add a light component for glow effect
            Light glowLight = gameObject.AddComponent<Light>();
            glowLight.color = glowColor;
            glowLight.intensity = glowIntensity;
            glowLight.range = 2f;
        }
        
        // Initialize UI
        SetupUI();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the player entered the trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            currentPlayer = other.gameObject;
            ShowPickupPrompt();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if the player left the trigger
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayer = null;
            HidePickupPrompt();
        }
    }
    
    // Optional: Add a rotating effect to make the battery more noticeable
    void Update()
    {
        // Check for pickup input when player is in range
        if (playerInRange && currentPlayer != null && Input.GetKeyDown(pickupKey))
        {
            TryPickupBattery();
        }
        
        // Floating effect
        if (enableFloating)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(startPosition.x, newY, startPosition.z);
        }
        
        // Rotation effect
        transform.Rotate(Vector3.up, 50f * Time.deltaTime);
    }
    
    private void TryPickupBattery()
    {
        if (currentPlayer != null)
        {
            // Find the flashlight script on the player or its children
            flashlight playerFlashlight = currentPlayer.GetComponentInChildren<flashlight>();
            
            if (playerFlashlight != null)
            {
                // Add battery to the flashlight
                playerFlashlight.AddBattery(batteryAmount);
                
                // Play pickup sound if available
                if (pickupSound != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupSoundVolume);
                }
                
                // Hide prompt before destroying
                HidePickupPrompt();
                
                // Destroy the battery pickup if set to do so
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    private void SetupUI()
    {
        // Create UI if not assigned
        if (pickupPromptUI == null)
        {
            CreatePickupPromptUI();
        }
        
        // Initially hide the prompt
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }
    }
    
    private void CreatePickupPromptUI()
    {
        // Find or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("BatteryPickupCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Ensure it's on top
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create UI GameObject
        pickupPromptUI = new GameObject("BatteryPickupPrompt");
        pickupPromptUI.transform.SetParent(canvas.transform, false);
        
        // Add TextMeshPro component
        promptText = pickupPromptUI.AddComponent<TextMeshProUGUI>();
        promptText.text = promptMessage;
        promptText.fontSize = 28;
        promptText.color = Color.white;
        promptText.alignment = TextAlignmentOptions.Center;
        
        // Add outline for better visibility
        promptText.fontStyle = FontStyles.Bold;
        promptText.outlineColor = Color.black;
        promptText.outlineWidth = 0.3f;
        
        // Position the text in the center of the screen
        RectTransform rectTransform = pickupPromptUI.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0, -150); // Below center
        rectTransform.sizeDelta = new Vector2(500, 60);
        
        // Initially hide
        pickupPromptUI.SetActive(false);
    }
    
    private void ShowPickupPrompt()
    {
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(true);
        }
    }
    
    private void HidePickupPrompt()
    {
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up UI when this object is destroyed
        if (pickupPromptUI != null)
        {
            Destroy(pickupPromptUI);
        }
    }
}
