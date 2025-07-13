using UnityEngine;

/// <summary>
/// Extension class that adds inventory functionality to the FirstPersonController
/// This acts as a bridge between the inventory system and the player controller
/// </summary>
public class FirstPersonInventoryController : MonoBehaviour
{
    [Header("Inventory Integration")]
    [Tooltip("Reference to the inventory system")]
    public InventorySystem inventorySystem;
    
    [Tooltip("Should movement be disabled when inventory is open?")]
    public bool disableMovementInInventory = true;
    
    [Tooltip("Should sprint be disabled when inventory is open?")]
    public bool disableSprintInInventory = true;
    
    [Tooltip("Should jumping be disabled when inventory is open?")]
    public bool disableJumpInInventory = true;
    
    // References
    private FirstPersonController playerController;
    
    // Original controller states to restore
    private bool originalPlayerCanMove;
    private bool originalEnableSprint;
    private bool originalEnableJump;
    private bool originalCameraCanMove;
    
    void Start()
    {
        // Get references
        playerController = GetComponent<FirstPersonController>();
        
        if (inventorySystem == null)
        {
            inventorySystem = GetComponent<InventorySystem>();
        }
        
        if (playerController == null)
        {
            Debug.LogError("FirstPersonInventoryController: No FirstPersonController found on this GameObject!");
            return;
        }
        
        if (inventorySystem == null)
        {
            Debug.LogError("FirstPersonInventoryController: No InventorySystem found! Please assign one or add InventorySystem component.");
            return;
        }
        
        // Store original states
        StoreOriginalStates();
        
        // Subscribe to inventory events
        inventorySystem.OnInventoryToggled += OnInventoryToggled;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryToggled -= OnInventoryToggled;
        }
    }
    
    private void StoreOriginalStates()
    {
        if (playerController != null)
        {
            originalPlayerCanMove = playerController.playerCanMove;
            originalEnableSprint = playerController.enableSprint;
            originalEnableJump = playerController.enableJump;
            originalCameraCanMove = playerController.cameraCanMove;
        }
    }
    
    private void OnInventoryToggled(bool isOpen)
    {
        if (playerController == null)
            return;
        
        if (isOpen)
        {
            // Inventory opened - disable player controls as specified
            if (disableMovementInInventory)
            {
                playerController.playerCanMove = false;
            }
            
            if (disableSprintInInventory)
            {
                playerController.enableSprint = false;
            }
            
            if (disableJumpInInventory)
            {
                playerController.enableJump = false;
            }
            
            // Camera movement is handled by InventorySystem
        }
        else
        {
            // Inventory closed - restore original states
            if (disableMovementInInventory)
            {
                playerController.playerCanMove = originalPlayerCanMove;
            }
            
            if (disableSprintInInventory)
            {
                playerController.enableSprint = originalEnableSprint;
            }
            
            if (disableJumpInInventory)
            {
                playerController.enableJump = originalEnableJump;
            }
            
            // Camera movement is handled by InventorySystem
        }
    }
    
    // Public methods for easy access to inventory functionality
    
    /// <summary>
    /// Add an item to the player's inventory
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <param name="itemDescription">Description of the item</param>
    /// <param name="itemIcon">Icon sprite for the item</param>
    /// <param name="quantity">Quantity to add</param>
    /// <param name="isStackable">Can this item be stacked?</param>
    /// <param name="maxStackSize">Maximum stack size</param>
    /// <returns>True if item was successfully added</returns>
    public bool AddItem(string itemName, string itemDescription, Sprite itemIcon, int quantity = 1, bool isStackable = false, int maxStackSize = 1)
    {
        if (inventorySystem == null)
            return false;
        
        InventoryItem item = new InventoryItem(itemName, itemDescription, itemIcon, quantity, isStackable, maxStackSize);
        return inventorySystem.AddItem(item);
    }
    
    /// <summary>
    /// Add an already created inventory item
    /// </summary>
    /// <param name="item">The inventory item to add</param>
    /// <returns>True if item was successfully added</returns>
    public bool AddItem(InventoryItem item)
    {
        if (inventorySystem == null)
            return false;
        
        return inventorySystem.AddItem(item);
    }
    
    /// <summary>
    /// Remove an item from the player's inventory
    /// </summary>
    /// <param name="itemName">Name of the item to remove</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <returns>True if item was successfully removed</returns>
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        if (inventorySystem == null)
            return false;
        
        return inventorySystem.RemoveItem(itemName, quantity);
    }
    
    /// <summary>
    /// Check if the player has a specific item
    /// </summary>
    /// <param name="itemName">Name of the item to check</param>
    /// <param name="quantity">Minimum quantity required</param>
    /// <returns>True if player has the item in required quantity</returns>
    public bool HasItem(string itemName, int quantity = 1)
    {
        if (inventorySystem == null)
            return false;
        
        return inventorySystem.HasItem(itemName, quantity);
    }
    
    /// <summary>
    /// Get the total quantity of a specific item
    /// </summary>
    /// <param name="itemName">Name of the item</param>
    /// <returns>Total quantity of the item</returns>
    public int GetItemCount(string itemName)
    {
        if (inventorySystem == null)
            return 0;
        
        return inventorySystem.GetItemCount(itemName);
    }
    
    /// <summary>
    /// Toggle the inventory open/closed
    /// </summary>
    public void ToggleInventory()
    {
        if (inventorySystem != null)
        {
            inventorySystem.ToggleInventory();
        }
    }
    
    /// <summary>
    /// Check if the inventory is currently open
    /// </summary>
    /// <returns>True if inventory is open</returns>
    public bool IsInventoryOpen()
    {
        if (inventorySystem == null)
            return false;
        
        return inventorySystem.IsInventoryOpen;
    }
    
    /// <summary>
    /// Get the number of empty slots in the inventory
    /// </summary>
    /// <returns>Number of empty slots</returns>
    public int GetEmptySlotCount()
    {
        if (inventorySystem == null)
            return 0;
        
        return inventorySystem.GetEmptySlotCount();
    }
    
    // Public properties for easy access
    public InventorySystem Inventory => inventorySystem;
    public FirstPersonController PlayerController => playerController;
}
