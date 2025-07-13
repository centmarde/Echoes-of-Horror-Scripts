using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public int quantity = 1;
    public int maxStackSize = 1;
    public bool isStackable = false;
    
    public InventoryItem(string name, string description, Sprite icon, int qty = 1, bool stackable = false, int maxStack = 1)
    {
        itemName = name;
        itemDescription = description;
        itemIcon = icon;
        quantity = qty;
        isStackable = stackable;
        maxStackSize = maxStack;
    }
    
    public InventoryItem Clone()
    {
        return new InventoryItem(itemName, itemDescription, itemIcon, quantity, isStackable, maxStackSize);
    }
}

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Maximum number of items the inventory can hold")]
    public int inventorySize = 12;
    
    [Tooltip("Key to open/close inventory")]
    public KeyCode inventoryKey = KeyCode.I; //key here
    
    [Header("UI References")]
    [Tooltip("Main inventory canvas")]
    public Canvas inventoryCanvas;
    
    [Tooltip("Parent object containing all inventory slots")]
    public Transform slotsParent;
    
    [Tooltip("Prefab for inventory slot UI")]
    public GameObject slotPrefab;
    
    [Tooltip("TextMeshPro component to display item information")]
    public TextMeshProUGUI itemInfoText;
    
    [Tooltip("TextMeshPro component to display item name")]
    public TextMeshProUGUI itemNameText;
    
    [Tooltip("Image component to display item icon")]
    public Image itemIconImage;
    
    [Header("Audio")]
    [Tooltip("Sound to play when picking up items")]
    public AudioClip pickupSound;
    
    [Tooltip("Sound to play when dropping items")]
    public AudioClip dropSound;
    
    [Tooltip("Sound to play when opening/closing inventory")]
    public AudioClip inventoryToggleSound;
    
    // Private variables
    private List<InventoryItem> items = new List<InventoryItem>();
    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private bool isInventoryOpen = false;
    private FirstPersonController playerController;
    private AudioSource audioSource;
    
    // Events
    public System.Action<InventoryItem> OnItemAdded;
    public System.Action<InventoryItem> OnItemRemoved;
    public System.Action<bool> OnInventoryToggled;

    void Start()
    {
        // Get references
        playerController = GetComponent<FirstPersonController>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize inventory
        InitializeInventory();
        
        // Start with inventory closed
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Handle inventory toggle input
        if (Input.GetKeyDown(inventoryKey))
        {
            ToggleInventory();
        }
    }
    
    void InitializeInventory()
    {
        if (slotsParent == null || slotPrefab == null)
        {
            Debug.LogError("InventorySystem: Missing slot parent or slot prefab references!");
            return;
        }
        
        // Clear existing slots
        foreach (Transform child in slotsParent)
        {
            DestroyImmediate(child.gameObject);
        }
        inventorySlots.Clear();
        
        // Create inventory slots
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            
            if (slot == null)
            {
                slot = slotObj.AddComponent<InventorySlot>();
            }
            
            slot.Initialize(i, this);
            inventorySlots.Add(slot);
        }
        
        // Initialize items list
        items = new List<InventoryItem>(new InventoryItem[inventorySize]);
    }
    
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryCanvas != null)
        {
            inventoryCanvas.gameObject.SetActive(isInventoryOpen);
        }
        
        // Control cursor based on inventory state
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Disable player camera movement when inventory is open
            if (playerController != null)
            {
                playerController.cameraCanMove = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Re-enable player camera movement
            if (playerController != null)
            {
                playerController.cameraCanMove = true;
            }
            
            // Clear item info display
            ClearItemInfo();
        }
        
        // Play sound
        PlaySound(inventoryToggleSound);
        
        // Trigger event
        OnInventoryToggled?.Invoke(isInventoryOpen);
    }
    
    public bool AddItem(InventoryItem newItem)
    {
        if (newItem == null) return false;
        
        // Try to stack with existing items if stackable
        if (newItem.isStackable)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null && 
                    items[i].itemName == newItem.itemName && 
                    items[i].quantity < items[i].maxStackSize)
                {
                    int spaceInStack = items[i].maxStackSize - items[i].quantity;
                    int amountToAdd = Mathf.Min(spaceInStack, newItem.quantity);
                    
                    items[i].quantity += amountToAdd;
                    newItem.quantity -= amountToAdd;
                    
                    // Update UI
                    inventorySlots[i].UpdateSlot(items[i]);
                    
                    if (newItem.quantity <= 0)
                    {
                        PlaySound(pickupSound);
                        OnItemAdded?.Invoke(newItem);
                        return true;
                    }
                }
            }
        }
        
        // Find empty slot
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] = newItem.Clone();
                inventorySlots[i].UpdateSlot(items[i]);
                
                PlaySound(pickupSound);
                OnItemAdded?.Invoke(newItem);
                return true;
            }
        }
        
        Debug.Log("Inventory is full!");
        return false;
    }
    
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null && items[i].itemName == itemName)
            {
                if (items[i].quantity >= quantity)
                {
                    items[i].quantity -= quantity;
                    
                    if (items[i].quantity <= 0)
                    {
                        InventoryItem removedItem = items[i];
                        items[i] = null;
                        inventorySlots[i].UpdateSlot(null);
                        OnItemRemoved?.Invoke(removedItem);
                    }
                    else
                    {
                        inventorySlots[i].UpdateSlot(items[i]);
                    }
                    
                    PlaySound(dropSound);
                    return true;
                }
            }
        }
        return false;
    }
    
    public bool RemoveItemAtIndex(int index)
    {
        if (index >= 0 && index < items.Count && items[index] != null)
        {
            InventoryItem removedItem = items[index];
            items[index] = null;
            inventorySlots[index].UpdateSlot(null);
            
            PlaySound(dropSound);
            OnItemRemoved?.Invoke(removedItem);
            return true;
        }
        return false;
    }
    
    public bool HasItem(string itemName, int quantity = 1)
    {
        int totalQuantity = 0;
        
        foreach (InventoryItem item in items)
        {
            if (item != null && item.itemName == itemName)
            {
                totalQuantity += item.quantity;
                if (totalQuantity >= quantity)
                    return true;
            }
        }
        
        return false;
    }
    
    public int GetItemCount(string itemName)
    {
        int totalQuantity = 0;
        
        foreach (InventoryItem item in items)
        {
            if (item != null && item.itemName == itemName)
            {
                totalQuantity += item.quantity;
            }
        }
        
        return totalQuantity;
    }
    
    public List<InventoryItem> GetAllItems()
    {
        List<InventoryItem> allItems = new List<InventoryItem>();
        
        foreach (InventoryItem item in items)
        {
            if (item != null)
            {
                allItems.Add(item);
            }
        }
        
        return allItems;
    }
    
    public void DisplayItemInfo(InventoryItem item)
    {
        if (item == null)
        {
            ClearItemInfo();
            return;
        }
        
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
        }
        
        if (itemInfoText != null)
        {
            string info = item.itemDescription;
            if (item.isStackable && item.quantity > 1)
            {
                info += $"\nQuantity: {item.quantity}";
            }
            itemInfoText.text = info;
        }
        
        if (itemIconImage != null)
        {
            if (item.itemIcon != null)
            {
                itemIconImage.sprite = item.itemIcon;
                itemIconImage.color = Color.white;
            }
            else
            {
                itemIconImage.sprite = null;
                itemIconImage.color = Color.clear;
            }
        }
    }
    
    public void ClearItemInfo()
    {
        if (itemNameText != null)
            itemNameText.text = "";
        
        if (itemInfoText != null)
            itemInfoText.text = "";
        
        if (itemIconImage != null)
        {
            itemIconImage.sprite = null;
            itemIconImage.color = Color.clear;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    // Public getters
    public bool IsInventoryOpen => isInventoryOpen;
    public int InventorySize => inventorySize;
    public int GetEmptySlotCount()
    {
        int emptyCount = 0;
        foreach (InventoryItem item in items)
        {
            if (item == null)
                emptyCount++;
        }
        return emptyCount;
    }
}
