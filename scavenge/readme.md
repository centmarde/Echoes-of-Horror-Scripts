# Scavenging System - MainComponent.cs

## 📋 **Overview**
The MainComponent.cs is a comprehensive scavenging system for Unity that allows players to search objects for random items. When a player approaches a scavengeable object and presses "Y", they enter a timed scavenging sequence with a loading screen and have a chance to find various items.

## 🎯 **Key Features:**

### **Collision & Interaction System:**
- Detects when player enters/exits the scavenge area using trigger colliders
- Press "Y" key to start scavenging when in range
- Visual highlighting when player is nearby (customizable color)
- Limited uses per scavenge object (default: 3 times)
- Automatic cleanup when object is fully scavenged

### **Loading System with Spinner:**
- Full-screen loading overlay with semi-transparent background
- Rotating spinner animation during scavenging
- Animated loading text ("Scavenging...")
- Progress bar showing scavenge completion percentage
- Configurable scavenge time (default: 3 seconds)

### **Random Item Generation:**
- **Battery Items** - Adds battery power to player's flashlight (integrates with existing flashlight.cs)
- **Health Items** - Restores player health (ready for integration with health system)
- **Key Items** - Special story items with unique IDs for quest/progression systems
- **Generic Items** - Spawns physical item prefabs in the world
- Weighted drop system with individual item chances and quantities
- Overall item drop chance setting (default: 70%)

### **Audio & Visual Effects:**
- Separate audio clips for scavenge start, success, and failure events
- Object highlighting when player is in range (customizable color)
- Optional particle effects during scavenging process
- Color-coded result messages (yellow for batteries, green for health, etc.)

### **UI System:**
- Auto-creates Canvas if none exists in the scene
- Interaction prompts ("Press Y to scavenge") with customizable text
- Professional loading screen with spinner and progress bar
- Result messages showing what items were found
- Automatic UI cleanup when objects are destroyed

## 🔧 **Setup Instructions:**

### **Basic Setup:**
1. **Attach the script** to any GameObject you want to be scavengeable (crates, drawers, cabinets, etc.)
2. **Ensure Player Tag** - Make sure your player GameObject has the "Player" tag
3. **Collider Setup** - The script will auto-add a BoxCollider with isTrigger=true if none exists

### **Inspector Configuration:**

#### **Scavenge Settings:**
- `Scavenge Key`: Key to press for scavenging (default: Y)
- `Scavenge Time`: How long the scavenging process takes (default: 3 seconds)
- `Interaction Range`: Distance for trigger collider (default: 4 units)
- `Max Scavenge Uses`: How many times this object can be scavenged (default: 3)

#### **Audio Settings:**
- `Scavenge Start Sound`: Audio clip played when starting to scavenge
- `Scavenge Complete Sound`: Audio clip played when successfully finding items
- `Scavenge Fail Sound`: Audio clip played when nothing is found
- `Audio Volume`: Volume multiplier for all sounds (default: 1.0)

#### **Visual Effects:**
- `Scavenge Effect`: Optional GameObject (particle system) activated during scavenging
- `Highlight Color`: Color used to highlight the object when player is nearby
- `Enable Outline`: Toggle object highlighting on/off

#### **Item Generation:**
- `Item Drop Chance`: Overall chance to find any item (0.0 - 1.0, default: 0.7)
- `Possible Items`: List of ScavengeItem objects with individual settings

### **ScavengeItem Configuration:**
Each item in the Possible Items list has these properties:
- `Item Name`: Display name for the item
- `Item Prefab`: GameObject to spawn (for physical items)
- `Drop Chance`: Individual chance for this item (0.0 - 1.0)
- `Min/Max Quantity`: Random quantity range when found

#### **Special Item Types:**
- **Battery Items**: Set `Is Battery = true`, configure `Battery Amount`
- **Health Items**: Set `Is Health Item = true`, configure `Health Amount`
- **Key Items**: Set `Is Key Item = true`, set unique `Key Item ID`

## 🎁 **Default Sample Items:**
The system comes with pre-configured sample items:
- **Batteries** (40% chance) - Adds 25 battery power to flashlight
- **Health Packs** (30% chance) - Restores 20 health points
- **Key Cards** (10% chance) - Story/progression items
- **Emergency Flares** (15% chance) - Generic items

## 🔌 **Integration Notes:**

### **Flashlight Integration:**
- Automatically integrates with existing `flashlight.cs` script
- Battery items call `AddBattery(amount)` method on player's flashlight component

### **Health System Integration:**
- Ready for integration - modify `GiveHealthToPlayer()` method
- Example: `currentPlayer.GetComponent<PlayerHealth>().AddHealth(healthAmount)`

### **Inventory System Integration:**
- Key items ready for inventory integration
- Modify `GiveKeyItemToPlayer()` method for your inventory system

### **Performance Considerations:**
- UI elements are created once and reused
- Automatic cleanup prevents memory leaks
- Efficient collision detection using Unity's trigger system

## 🛠 **Customization Tips:**

### **Adding New Item Types:**
1. Create new boolean flags in ScavengeItem class (e.g., `isWeapon`, `isAmmo`)
2. Add corresponding amount fields (e.g., `weaponDamage`, `ammoCount`)
3. Implement handling in `GenerateRandomItem()` method
4. Add appropriate give methods (e.g., `GiveWeaponToPlayer()`)

### **Custom UI Styling:**
- Modify `CreateLoadingUI()` and `CreateScavengePromptUI()` methods
- Change colors, fonts, and positioning
- Add custom sprites for spinner and progress bar

### **Advanced Features:**
- Add sound effects with random pitch variation
- Implement item rarity system with color coding
- Add animation tweens for smoother UI transitions
- Create area-specific item pools

## 🚀 **Quick Start Example:**
1. Create an empty GameObject in your scene
2. Add a 3D model (like a crate or box)
3. Attach the MainComponent script
4. Configure scavenge time and item drop rates
5. Test with your player controller

The system is designed to work seamlessly with existing Unity horror game systems and provides a solid foundation for item collection mechanics.