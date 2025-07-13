using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public Image backgroundImage;
    
    [Header("Visual Settings")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color selectedColor = Color.green;
    
    // Private variables
    private InventoryItem currentItem;
    private int slotIndex;
    private InventorySystem inventorySystem;
    private bool isSelected = false;
    
    public void Initialize(int index, InventorySystem inventory)
    {
        slotIndex = index;
        inventorySystem = inventory;
        
        // Get UI components if not assigned
        if (itemIcon == null)
            itemIcon = transform.Find("ItemIcon")?.GetComponent<Image>();
        
        if (quantityText == null)
            quantityText = transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        // Set initial state
        UpdateSlot(null);
    }
    
    public void UpdateSlot(InventoryItem item)
    {
        currentItem = item;
        
        if (item != null)
        {
            // Show item icon
            if (itemIcon != null)
            {
                itemIcon.sprite = item.itemIcon;
                itemIcon.color = item.itemIcon != null ? Color.white : Color.clear;
                itemIcon.gameObject.SetActive(item.itemIcon != null);
            }
            
            // Show quantity if stackable and more than 1
            if (quantityText != null)
            {
                if (item.isStackable && item.quantity > 1)
                {
                    quantityText.text = item.quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // Empty slot
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.color = Color.clear;
                itemIcon.gameObject.SetActive(false);
            }
            
            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }
        }
        
        // Update background color
        UpdateBackgroundColor();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && inventorySystem != null)
        {
            inventorySystem.DisplayItemInfo(currentItem);
        }
        
        if (backgroundImage != null && !isSelected)
        {
            backgroundImage.color = highlightColor;
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventorySystem != null)
        {
            inventorySystem.ClearItemInfo();
        }
        
        UpdateBackgroundColor();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem != null)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                // Left click - select/deselect
                SetSelected(!isSelected);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // Right click - remove item
                if (inventorySystem != null)
                {
                    inventorySystem.RemoveItemAtIndex(slotIndex);
                }
            }
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateBackgroundColor();
        
        // Deselect other slots if this one is selected
        if (selected && inventorySystem != null)
        {
            // You could implement multi-selection here if needed
        }
    }
    
    private void UpdateBackgroundColor()
    {
        if (backgroundImage != null)
        {
            if (isSelected)
            {
                backgroundImage.color = selectedColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }
    }
    
    // Public getters
    public InventoryItem GetItem() => currentItem;
    public int GetSlotIndex() => slotIndex;
    public bool HasItem() => currentItem != null;
    public bool IsSelected() => isSelected;
}
