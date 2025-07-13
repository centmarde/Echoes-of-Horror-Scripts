# Simple Inventory System for Unity
## Horror Game FirstPersonController Integration

This inventory system provides a complete, easy-to-use inventory solution that integrates seamlessly with your FirstPersonController. It includes text-based inventory display, item pickup functionality, and proper player control integration.

## Features

- **Simple Text-Based Inventory**: Clean text display showing items and quantities
- **5-Slot Maximum**: Limited to 5 inventory slots for focused gameplay
- **Item Stacking**: Support for stackable items with configurable stack sizes
- **Visual UI**: Canvas-based inventory interface with TextMeshPro
- **Item Pickup**: Complete pickup system with visual and audio feedback
- **Player Integration**: Seamless integration with FirstPersonController
- **Audio Support**: Pickup, drop, and UI sounds
- **Event System**: Callbacks for item addition, removal, and inventory state changes
- **Scavenge Integration**: Automatic item discovery and inventory addition

## Components

### 1. InventorySystem.cs
Main inventory logic and text-based UI management.

### 2. FirstPersonInventoryController.cs
Bridge component that integrates inventory with your FirstPersonController.

### 3. MainComponent.cs (Scavenge System Integration)
Scavenge system that automatically adds found items to the player's inventory.

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
   - Resize to fit text display (recommend 300x200)
   - Center it on screen

3. **Create Inventory Text Display**:
   - Right-click InventoryPanel → UI → Text - TextMeshPro
   - Name it "InventoryListText"
   - Set font size to 14-16
   - Set color to white
   - Enable "Word Wrap" if needed
   - Anchor to fill parent with margins
   - Set text alignment to Upper Left

4. **Create Item Info Panel** (Optional):
   - Right-click InventoryPanel → UI → Panel
   - Name it "ItemInfoPanel"
   - Position beside inventory text (resize to about 200x150)
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
   - Inventory Size: 5 (for 5-slot text-based inventory)
   - Inventory Key: I (changed from Tab to I key)
   - Inventory Canvas: Drag your InventoryCanvas
   - Inventory List Text: Drag your InventoryListText TextMeshPro component
   - Item Info Text: Drag ItemDescriptionText (from ItemInfoPanel, optional)
   - Item Name Text: Drag ItemNameText (from ItemInfoPanel, optional)
   - Item Icon Image: Drag ItemIconDisplay (from ItemInfoPanel, optional)

3. **Configure FirstPersonInventoryController**:
   - Inventory System: Should auto-assign
   - Disable Movement In Inventory: true
   - Disable Sprint In Inventory: true
   - Disable Jump In Inventory: true

### Step 3: Setup Scavenge System Integration

1. **Add MainComponent to Scavengeable Objects**:
   - Select objects you want to be scavengeable
   - Add MainComponent script to them
   - Configure the scavenge settings

2. **Configure MainComponent**:
   - Scavenge Key: Y (or your preferred key)
   - Scavenge Time: 3 seconds (time to scavenge)
   - Max Scavenge Uses: 3 (how many times object can be scavenged)
   - Item Drop Chance: 0.7 (70% chance to find items)
   - Add items to Possible Items list with:
     - Item Name, Description, Icon
     - Drop chance and quantity range
     - Inventory properties (stackable, max stack size)
     - Special properties (battery, health, key item)

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

### Text Display Format

The inventory will display items in this format:
```
INVENTORY:
• Key
• Battery x3
• Ammo x25
• Health Potion

[4/5]
```

### Adding Items via Script

```csharp
// Get the inventory system
InventorySystem inventorySystem = player.GetComponent<InventorySystem>();

// Create and add a simple item
InventoryItem keyItem = new InventoryItem
{
    itemName = "Key",
    itemDescription = "Opens doors",
    itemIcon = keyIcon,
    isStackable = false
};
inventorySystem.AddItem(keyItem);

// Create and add stackable item
InventoryItem ammoItem = new InventoryItem
{
    itemName = "Ammo",
    itemDescription = "Rifle ammunition", 
    itemIcon = ammoIcon,
    quantity = 30,
    isStackable = true,
    maxStackSize = 100
};
inventorySystem.AddItem(ammoItem);
```

### Integration with Scavenge System

The MainComponent scavenge system automatically creates and adds items to the inventory:

```csharp
// Items are automatically added when scavenging
// The text display updates automatically
// Players can press 'I' to view their inventory
// Maximum of 5 slots prevents inventory clutter
```

// Check if player has item
if (inventoryController.HasItem("Key"))
{
    // Player has a key
    inventoryController.RemoveItem("Key");
    OpenDoor();
}
```

### Using Scavenge System

```csharp
// On a scavengeable object with MainComponent
MainComponent scavenge = GetComponent<MainComponent>();

// Add custom items to scavenge list
ScavengeItem customItem = new ScavengeItem();
customItem.itemName = "Rare Artifact";
customItem.itemDescription = "A mysterious ancient artifact";
customItem.dropChance = 0.1f; // 10% chance
customItem.isStackable = false;
scavenge.possibleItems.Add(customItem);
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
   - Change `inventorySize` in InventorySystem (set to 5 for single row)
   - Adjust Grid Layout Group columns accordingly (set to 5)

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

4. **Scavenge system doesn't work**:
   - Verify player has "Player" tag
   - Check that MainComponent is on scavengeable objects
   - Ensure InventorySystem is on the player
   - Check scavenge range and collision setup

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
