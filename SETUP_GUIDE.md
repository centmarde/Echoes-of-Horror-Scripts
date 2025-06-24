# Enemy AI 2 Setup Guide - "Weeping Angel" Style Enemy

This guide explains how to set up the enemyAi2.cs system that creates enemies that stop moving when the player faces them (like the Weeping Angels from Doctor Who).

## Key Features

- **Instant Freeze Detection**: Enemy freezes immediately when player faces them (no cursor dependency)
- **No Roaming**: Enemy stands still when idle, only moves when chasing the player
- **Face-Based Detection**: Uses camera direction to detect when player is looking at enemy
- **Configurable Detection**: Adjustable detection angle and distance

## Files Created

1. **enemyAi2.cs** - Modified enemy AI that freezes when player faces them
2. **PlayerWatchingDetector.cs** - Optional detector component (not required for basic functionality)
3. **EnemyWatchingManager.cs** - Optional manager for multiple enemies

## Setup Instructions

### Enemy Setup

**GameObject:** Your Enemy object (monster/creature)

**Required Components:**
- **enemyAi2.cs** (attach this instead of enemyAi.cs)
- Rigidbody (for physics movement)
- Collider (for detection and physics)
- Animator (optional, for animations)

**enemyAi2 Settings:**
- **Player:** Drag your player object here (usually auto-detected)
- **Watching Detector:** Leave empty (built-in detection is used)
- **Completely Stop When Watched:** True (enemy freezes completely) 
- **Freeze Grace Period:** 0.5 seconds (how long after looking away before enemy moves again)

**Copy these settings from your existing enemyAi.cs:**
- All Follow Settings (speeds, distances, etc.)
- All Vision Settings (vision angle, line of sight, etc.)
- Animation references
- Ground Detection settings

**Note:** 
- No PlayerWatchingDetector component is needed
- No red crosshair will appear - the system works invisibly
- Roaming settings are no longer needed - the enemy will stand still when idle

### 3. Scene Manager Setup (Optional but Recommended)

**GameObject:** Create an empty GameObject called "EnemyWatchingManager"

**Required Components:**
- **EnemyWatchingManager.cs** (attach this)

**EnemyWatchingManager Settings:**
- **Watching Detector:** Drag your player object here
- **Auto Discover Enemies:** True (automatically finds all enemyAi2 enemies)
- **Freeze Sound:** Audio clip to play when enemy freezes
- **Unfreeze Sound:** Audio clip to play when enemy unfreezes

## Step-by-Step Setup Process

### Enemy Setup
1. Select your Enemy GameObject in the hierarchy
2. Remove the old "enemyAi" component (right-click it and select "Remove Component")
3. Add the "enemyAi2" component instead
4. Copy over your settings from the old component (speeds, distances, etc.)
5. Set the "Player" field to your player GameObject
6. Configure the freeze behavior settings
7. **Important:** Remove any roaming-related settings as they are no longer used

### Optional Manager Setup (Advanced)
1. Create an empty GameObject in the scene
2. Name it "EnemyWatchingManager"
3. Add the "EnemyWatchingManager" component
4. Set "Auto Discover Enemies" to true
5. Assign audio clips for freeze/unfreeze sounds if desired

## How It Works

1. **Face Detection:** The enemy continuously checks if the player's camera is facing towards it
2. **Instant Freeze:** When the player faces the enemy (within detection angle and distance), it freezes immediately
3. **No Visual Feedback:** The system works invisibly - no crosshairs or UI elements
4. **No Roaming:** When idle, the enemy stands perfectly still at its spawn position
5. **Chase Behavior:** When the player is detected but not facing the enemy, it will chase normally
6. **Unfreeze Delay:** When the player looks away, there's a grace period before the enemy starts moving again

## Key Differences from Original

- **No Cursor Dependency:** Uses camera direction instead of precise cursor targeting
- **No Roaming:** Enemy doesn't wander around when idle
- **Instant Detection:** Freezes immediately when player faces it (no delay)
- **Simplified Setup:** No need for PlayerWatchingDetector component
- **Face-Based:** Uses broader face detection rather than precise cursor aiming

## Customization Options

### Per-Enemy Customization
Each enemy can have different watching behavior:
- Some enemies freeze completely
- Some enemies only slow down
- Different unfreeze delays
- Different detection distances

### Global Settings
The EnemyWatchingManager allows you to:
- Control all enemies at once
- Play sounds when enemies freeze/unfreeze
- Get statistics about watched enemies
- Override individual enemy settings

## Animation Setup

If you want your enemies to have different animations when frozen:

1. In your Animator Controller, add a new bool parameter called "isFrozen"
2. Create a "Frozen" animation state
3. Add transitions between normal movement and frozen states
4. The enemyAi2 script will automatically set the "isFrozen" parameter

## Testing

1. Play the scene
2. Look directly at an enemy - it should freeze and show a red crosshair
3. Look away - after the grace period, the enemy should start moving again
4. Check the console for debug messages if "Show Debug Info" is enabled

## Troubleshooting

**Enemy doesn't freeze:**
- Check that PlayerWatchingDetector is attached to the player
- Verify the detection distance is high enough
- Make sure the enemy has the enemyAi2 component

**Crosshair doesn't appear:**
- Check that "Show Watching Crosshair" is enabled in PlayerWatchingDetector
- Verify the enemy is within detection distance and angle

**Enemy unfreezes too quickly/slowly:**
- Adjust the "Freeze Grace Period" in enemyAi2
- Modify the "Unfreeze Delay" in EnemyWatchingManager

## Performance Notes

- The detection system uses raycasting, which is relatively efficient
- Multiple enemies are supported without significant performance impact
- The system automatically cleans up references to destroyed objects

## Advanced Features

- **Line of Sight:** Enemies behind walls won't be detected
- **Distance-Based:** Only nearby enemies are affected
- **Audio Feedback:** Optional sounds when enemies freeze/unfreeze
- **Manager Override:** Global control over all enemies
- **Debug Visualization:** Gizmos show detection ranges and watched enemies
