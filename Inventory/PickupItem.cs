using UnityEngine;
using TMPro;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("Name of the item")]
    public string itemName = "Item";
    
    [Tooltip("Description of the item")]
    [TextArea(3, 5)]
    public string itemDescription = "A useful item.";
    
    [Tooltip("Icon sprite for the item")]
    public Sprite itemIcon;
    
    [Tooltip("Quantity of this item")]
    public int quantity = 1;
    
    [Tooltip("Can this item be stacked?")]
    public bool isStackable = false;
    
    [Tooltip("Maximum stack size")]
    public int maxStackSize = 1;
    
    [Header("Pickup Settings")]
    [Tooltip("Maximum distance to pick up the item")]
    public float pickupRange = 3f;
    
    [Tooltip("Key to pick up the item")]
    public KeyCode pickupKey = KeyCode.E;
    
    [Tooltip("Show pickup prompt when in range")]
    public bool showPickupPrompt = true;
    
    [Header("Visual Settings")]
    [Tooltip("Should the item rotate?")]
    public bool rotateItem = true;
    
    [Tooltip("Rotation speed")]
    public float rotationSpeed = 50f;
    
    [Tooltip("Should the item bob up and down?")]
    public bool bobItem = true;
    
    [Tooltip("Bob height")]
    public float bobHeight = 0.3f;
    
    [Tooltip("Bob speed")]
    public float bobSpeed = 2f;
    
    [Header("Audio")]
    [Tooltip("Sound to play when picked up")]
    public AudioClip pickupSound;
    
    [Header("UI")]
    [Tooltip("Canvas to show pickup prompt")]
    public Canvas pickupPromptCanvas;
    
    [Tooltip("TextMeshPro component for pickup prompt")]
    public TextMeshProUGUI pickupPromptText;
    
    // Private variables
    private Transform playerTransform;
    private InventorySystem playerInventory;
    private Vector3 startPosition;
    private bool playerInRange = false;
    private AudioSource audioSource;
    
    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerInventory = player.GetComponent<InventorySystem>();
        }
        
        // Store starting position for bobbing
        startPosition = transform.position;
        
        // Get audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && pickupSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize pickup prompt
        if (pickupPromptCanvas != null)
        {
            pickupPromptCanvas.gameObject.SetActive(false);
        }
        
        if (pickupPromptText != null && showPickupPrompt)
        {
            pickupPromptText.text = $"Press {pickupKey} to pick up {itemName}";
        }
    }
    
    void Update()
    {
        // Check player distance
        CheckPlayerDistance();
        
        // Handle pickup input
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
        
        // Visual effects
        if (rotateItem)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        if (bobItem)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    void CheckPlayerDistance()
    {
        if (playerTransform == null)
            return;
        
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= pickupRange;
        
        // Show/hide pickup prompt
        if (showPickupPrompt && pickupPromptCanvas != null)
        {
            if (playerInRange && !wasInRange)
            {
                pickupPromptCanvas.gameObject.SetActive(true);
            }
            else if (!playerInRange && wasInRange)
            {
                pickupPromptCanvas.gameObject.SetActive(false);
            }
        }
    }
    
    void TryPickup()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("No InventorySystem found on player!");
            return;
        }
        
        // Create inventory item
        InventoryItem item = new InventoryItem(
            itemName,
            itemDescription,
            itemIcon,
            quantity,
            isStackable,
            maxStackSize
        );
        
        // Try to add to inventory
        if (playerInventory.AddItem(item))
        {
            // Play pickup sound
            if (audioSource != null && pickupSound != null)
            {
                // Play sound at pickup location and destroy after sound finishes
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Destroy the pickup object
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"Cannot pick up {itemName}: Inventory is full!");
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw pickup range in editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
    
    // Public method to set item data programmatically
    public void SetItemData(string name, string description, Sprite icon, int qty = 1, bool stackable = false, int maxStack = 1)
    {
        itemName = name;
        itemDescription = description;
        itemIcon = icon;
        quantity = qty;
        isStackable = stackable;
        maxStackSize = maxStack;
        
        if (pickupPromptText != null && showPickupPrompt)
        {
            pickupPromptText.text = $"Press {pickupKey} to pick up {itemName}";
        }
    }
}
