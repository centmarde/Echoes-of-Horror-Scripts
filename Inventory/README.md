# Simple Inventory System for Unity
## Horror Game FirstPersonController Integration

This inventory system provides a complete, easy-to-use inventory solution that integrates seamlessly with your FirstPersonController. It includes UI management, item pickup functionality, and proper player control integration.

## Features

- **Simple Grid-Based Inventory**: Configurable slot-based inventory system
- **Item Stacking**: Support for stackable items with configurable stack sizes
- **Visual UI**: Canvas-based inventory interface with hover effects
- **Item Pickup**: Complete pickup system with visual and audio feedback
- **Player Integration**: Seamless integration with FirstPersonController
- **Audio Support**: Pickup, drop, and UI sounds
- **Mouse Controls**: Full mouse support when inventory is open
- **Event System**: Callbacks for item addition, removal, and inventory state changes

## Components

### 1. InventorySystem.cs
Main inventory logic and UI management.

### 2. InventorySlot.cs
Individual slot behavior and mouse interactions.

### 3. PickupItem.cs
World items that can be picked up by the player.

### 4. FirstPersonInventoryController.cs
Bridge component that integrates inventory with your FirstPersonController.

---

## Setup Instructions

**Important**: This inventory system uses TextMeshPro for all text components. Make sure you have TextMeshPro imported:
- Go to Window → TextMeshPro → Import TMP Essential Resources
- If prompted, import the examples and extras as well

### Step 1: Create the Inventory UI

1. **Create Main Canvas**:
   - Right-click in Hierarchy → UI → Canvas
   - Name it "InventoryCanvas"
   - Set Canvas Scaler to "Scale With Screen Size"

2. **Create Inventory Panel**:
   - Right-click InventoryCanvas → UI → Panel
   - Name it "InventoryPanel"
   - Resize to your preferred size (recommend 400x300)
   - Center it on screen

3. **Create Slots Grid**:
   - Right-click InventoryPanel → UI → Panel
   - Name it "SlotsParent"
   - Add Grid Layout Group component:
     - Cell Size: 60x60
     - Spacing: 5x5
     - Child Alignment: Upper Left
     - Constraint: Fixed Column Count (4 recommended)

4. **Create Slot Prefab**:
   - Right-click SlotsParent → UI → Image
   - Name it "InventorySlot"
   - Set Image color to semi-transparent (e.g., 0.2 alpha)
   - Add child Image named "ItemIcon":
     - Anchor to fill parent with small margins
     - Set Image color to white
     - Disable "Raycast Target"
   - Add child TextMeshPro - Text (UI) named "QuantityText":
     - Anchor to bottom-right corner
     - Small font size (12-14)
     - White color with outline
     - Disable "Raycast Target"

5. **Create Item Info Panel**:
   - Right-click InventoryPanel → UI → Panel
   - Name it "ItemInfoPanel"
   - Position on right side of inventory (resize to about 150x200)
   - Add child Image named "ItemIconDisplay":
     - Position at top of panel
     - Set size to 64x64 pixels
     - Set color to white
   - Add child TextMeshPro - Text (UI) named "ItemNameText":
     - Position below ItemIconDisplay
     - Set font size to 14-16 (larger font)
     - Set color to white
     - Set text alignment to Center
   - Add child TextMeshPro - Text (UI) named "ItemDescriptionText":
     - Position below ItemNameText
     - Set font size to 10-12
     - Set color to white or light gray
     - Enable "Word Wrap" in TextMeshPro component
     - Set text alignment to Upper Left

6. **Convert Slot to Prefab**:
   - Drag InventorySlot from Hierarchy to Project folder
   - Delete the InventorySlot from SlotsParent (keep the empty SlotsParent)

### Step 2: Setup Player Integration

1. **Add Components to Player**:
   ```
   Player GameObject:
   ├── FirstPersonController (existing)
   ├── InventorySystem (new)
   ├── FirstPersonInventoryController (new)
   └── AudioSource (for inventory sounds)
   ```

2. **Configure InventorySystem**:
   - Inventory Size: 12 (or desired number of slots)
   - Inventory Key: I (changed from Tab to I key)
   - Inventory Canvas: Drag your InventoryCanvas
   - Slots Parent: Drag your SlotsParent
   - Slot Prefab: Drag your InventorySlot prefab
   - Item Info Text: Drag ItemDescriptionText (from ItemInfoPanel)
   - Item Name Text: Drag ItemNameText (from ItemInfoPanel)
   - Item Icon Image: Drag ItemIconDisplay (from ItemInfoPanel)

3. **Configure FirstPersonInventoryController**:
   - Inventory System: Should auto-assign
   - Disable Movement In Inventory: true
   - Disable Sprint In Inventory: true
   - Disable Jump In Inventory: true

### Step 3: Create Pickup Items

1. **Create Pickup Item GameObject**:
   - Create an empty GameObject in your scene
   - Add a 3D model or primitive shape (e.g., Cube)
   - Add Collider component (set as Trigger if needed)
   - Add PickupItem component

2. **Configure PickupItem**:
   - Item Name: "Health Potion"
   - Item Description: "Restores health when used"
   - Item Icon: Drag a sprite (create one or use Unity default)
   - Quantity: 1
   - Is Stackable: true (if you want multiple in one slot)
   - Max Stack Size: 5
   - Pickup Range: 3
   - Pickup Key: E

3. **Optional - Create Pickup UI**:
   - Create World Space Canvas as child of pickup item
   - Add Panel with TextMeshPro - Text (UI) showing "Press E to pickup"
   - Assign to Pickup Prompt Canvas in PickupItem

### Step 4: Audio Setup (Optional)

1. **Find Audio Clips**:
   - Pickup sound (beep, coin sound, etc.)
   - Drop sound (thud, drop sound, etc.)
   - UI sound (click, whoosh, etc.)

2. **Assign to InventorySystem**:
   - Pickup Sound: Assign pickup audio clip
   - Drop Sound: Assign drop audio clip
   - Inventory Toggle Sound: Assign UI audio clip

---

## Usage Examples

### Adding Items via Script

```csharp
// Get the inventory controller
FirstPersonInventoryController inventoryController = player.GetComponent<FirstPersonInventoryController>();

// Add a simple item
inventoryController.AddItem("Key", "Opens doors", keyIcon);

// Add stackable item
inventoryController.AddItem("Ammo", "Rifle ammunition", ammoIcon, 30, true, 100);

// Check if player has item
if (inventoryController.HasItem("Key"))
{
    // Player has a key
    inventoryController.RemoveItem("Key");
    OpenDoor();
}
```

### Creating Items from Code

```csharp
// Create an inventory item
InventoryItem healthPotion = new InventoryItem(
    "Health Potion",
    "Restores 50 HP",
    healthPotionIcon,
    1,
    true,  // stackable
    5      // max stack size
);

// Add to inventory
inventoryController.AddItem(healthPotion);
```

### Pickup Item from Script

```csharp
// On a pickup item GameObject
PickupItem pickup = GetComponent<PickupItem>();
pickup.SetItemData("Flashlight", "Illuminates dark areas", flashlightIcon);
```

---

## Customization Options

### Visual Customization

1. **Slot Appearance**:
   - Modify colors in InventorySlot component
   - Normal Color: Default appearance
   - Highlight Color: When mouse hovers
   - Selected Color: When clicked

2. **UI Layout**:
   - Adjust Grid Layout Group settings
   - Change panel sizes and positions
   - Modify fonts and colors

### Functional Customization

1. **Inventory Size**:
   - Change `inventorySize` in InventorySystem
   - Adjust Grid Layout Group columns accordingly

2. **Pickup Controls**:
   - Change `pickupKey` in PickupItem components
   - Modify `pickupRange` for different interaction distances

3. **Player Control Integration**:
   - Toggle which controls are disabled in FirstPersonInventoryController
   - Modify camera behavior when inventory is open

---

## Troubleshooting

### Common Issues

1. **Inventory doesn't open**:
   - Check if InventoryCanvas is assigned
   - Verify Canvas is set to Screen Space - Overlay
   - Ensure no other scripts are capturing the I key (changed from Tab)

2. **Items don't show in slots**:
   - Verify Slot Prefab has InventorySlot component
   - Check if ItemIcon Image is assigned in slot prefab
   - Ensure ItemIcon child object has "Raycast Target" disabled

3. **Can't drag UI TextMeshPro components**:
   - Make sure you've created ItemNameText and ItemDescriptionText as children of ItemInfoPanel
   - These must be TextMeshPro - Text (UI) components (not legacy Text)
   - Check that the GameObjects are active in the hierarchy
   - Ensure TextMeshPro package is imported (Window → TextMeshPro → Import TMP Essential Resources)

4. **Pickup doesn't work**:
   - Verify player has "Player" tag
   - Check pickup range in PickupItem
   - Ensure InventorySystem is on the player

4. **Mouse cursor issues**:
   - FirstPersonController.lockCursor should be true
   - InventorySystem handles cursor state automatically

### Performance Tips

1. **Optimize UI**:
   - Use object pooling for slots if inventory size is very large
   - Disable Canvas when not in use

2. **Audio Optimization**:
   - Use AudioSource.PlayOneShot for pickup sounds
   - Keep audio clips short and compressed

---

## Events and Callbacks

### Available Events

```csharp
// In InventorySystem
public System.Action<InventoryItem> OnItemAdded;
public System.Action<InventoryItem> OnItemRemoved;
public System.Action<bool> OnInventoryToggled;

// Subscribe to events
inventorySystem.OnItemAdded += (item) => Debug.Log($"Added {item.itemName}");
inventorySystem.OnItemRemoved += (item) => Debug.Log($"Removed {item.itemName}");
inventorySystem.OnInventoryToggled += (isOpen) => Debug.Log($"Inventory {(isOpen ? "opened" : "closed")}");
```

---

## Integration with Other Systems

### Quest System Integration

```csharp
// Example: Quest requires collecting items
void CheckQuestItems()
{
    if (inventoryController.HasItem("Ancient Key", 1) && 
        inventoryController.HasItem("Magic Scroll", 1))
    {
        CompleteQuest();
    }
}
```

### Save System Integration

```csharp
// Example: Save inventory state
[System.Serializable]
public class InventorySaveData
{
    public List<InventoryItem> items;
}

public InventorySaveData SaveInventory()
{
    return new InventorySaveData 
    { 
        items = inventorySystem.GetAllItems() 
    };
}
```

---

## License

This inventory system is free to use and modify for your projects. No attribution required, but appreciated!

## Support

For issues or questions, check that all components are properly assigned and that the setup steps were followed correctly. The system is designed to be modular and easy to debug through Unity's Inspector.
