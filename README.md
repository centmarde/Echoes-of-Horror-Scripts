# Echoes of Horror Scripts Unity Project

This Unity project contains scripts for a first-person horror game with comprehensive player movement and interaction systems.

## Main Scripts

### FirstPersonController (playerScript.md)
A complete first-person player controller with the following features:

#### Core Movement System
- **Walking & Running**: Basic WASD movement with configurable walk speed
- **Sprint System**: Hold-to-sprint with stamina management
  - Configurable sprint key (default: Left Shift)
  - Sprint duration and cooldown mechanics
  - Sprint disable timer for balance
  - Field of view changes during sprint
- **Jump Mechanics**: Space bar jumping with ground detection
- **Crouch System**: Toggle or hold-to-crouch functionality
  - Configurable crouch key (default: Left Control)
  - Speed reduction while crouched
  - Scale transformation for visual feedback

#### Camera & Look System
- **Mouse Look**: Full 360° rotation with sensitivity controls
- **Camera Inversion**: Optional inverted Y-axis
- **Zoom Feature**: Right-click to zoom with FOV adjustment
- **Look Angle Limits**: Configurable min/max vertical look angles
- **Crosshair System**: Customizable crosshair with sprite and color options

#### UI Systems
- **Sprint Bar**: Visual stamina indicator
  - Configurable size and position
  - Hide when full option
  - Automatic fade in/out
- **Flashlight Bar**: Battery indicator for flashlight usage
  - Similar customization to sprint bar
  - Offset positioning from sprint bar
  - Integration with external flashlight systems

#### Advanced Features
- **Head Bob**: Realistic camera movement while walking
  - Configurable speed and amount
  - Joint-based animation system
- **Cursor Management**: Automatic cursor lock/unlock
- **Ground Detection**: Raycast-based ground checking
- **Custom Editor**: Complete Unity Inspector interface for easy configuration

#### Technical Implementation
- Uses Rigidbody for physics-based movement
- Modular design with separate regions for each feature
- Extensive customization options through public variables
- Built-in safety checks and null reference protection
- Unity Editor integration with custom inspector

## Version History
- **v1.0.1**: Enable/Disable Headbob, improved camera rotations
- **v1.0.2**: Removed unlimited sprint, enforced cooldown system  
- **v1.0.3**: Added flashlight bar integration

## Usage
This controller is designed for horror games requiring smooth first-person movement with stamina management and atmospheric features like flashlight integration. The script provides a solid foundation for survival horror gameplay mechanics.

## Additional Features

### Flashlight System (`flashlight.cs`)
- **Battery Management**: Rechargeable battery system with drain mechanics
  - Configurable battery duration and drain rate
  - Visual battery indicator integration with FirstPersonController
  - Battery pickup system for restocking
- **Enemy Detection**: Flashlight-based enemy detection and teleportation
  - Configurable detection distance and cone angle
  - Enemy teleport functionality when illuminated
- **Toggle Control**: R key to toggle flashlight on/off
- **UI Integration**: Automatic flashlight bar with fade in/out effects

### Door Interaction System
- **Single Door Controller** (`DoorInteraction.cs`): E key interaction for opening/closing doors
  - Configurable open angle and animation speed
  - Smooth rotation animations with Quaternion.Slerp
  - Trigger-based player detection
- **Double Door System** (`DoubleDoor.cs`): Synchronized dual door opening
  - Independent left and right door hinge controls
  - Simultaneous opening with E key interaction
  - Configurable opening angles for each door

### Enemy AI & Catch System
- **Advanced Enemy AI** (`enemyAi.cs`): Sophisticated enemy behavior system
  - Multiple AI states: Idle, Chasing, CatchingPlayer
  - Proximity detection and line-of-sight mechanics
  - Roaming behavior with configurable patrol patterns
  - Safe zone integration and avoidance
  - Speed modulation based on player visibility
- **Catch Management** (`CatchManager.cs` & `CatchSequenceManager.cs`):
  - Cinematic catch sequences with camera control
  - Player lifting and positioning during catch
  - Monster-player distance management
  - Spotlight effects and audio integration
  - Respawn system with configurable delay
- **Enemy Light Controller** (`EnemyLightController.cs`): Dynamic enemy lighting
  - Automatic light activation during catch sequences
  - Configurable start state and catch behavior

### Interactive Elements
- **Battery Pickup System** (`BatteryPickup.cs`):
  - F key interaction with range detection
  - Visual effects: floating animation and glow
  - UI prompt system with automatic canvas creation
  - Configurable battery amounts and pickup sounds
- **Jump Scare System** (`JumpScare1.cs`):
  - Triggered movement animations
  - Audio integration with configurable sound clips
  - Return-to-position mechanics with delays
  - Animator parameter control for complex sequences

### HUD & Event Management
- **Catch Counter** (`CatchCounter.cs`): Game progression tracking
  - Singleton pattern for persistent state
  - Configurable maximum catch limits
  - Scene transition on game over
  - UI integration with TextMeshPro
- **Player Animation Controller** (`PlayerAnimation.cs`):
  - WASD movement detection and animation
  - Walk/run state management
  - Speed parameter updates for smooth transitions

### Menu & Scene Management
- **Main Menu System** (`Menu.cs`):
  - Scene loading and transition management
  - Start game and quit functionality
  - Cursor state management for UI interaction
  - Error handling for scene operations

### Player Management
- **Spawn System** (`PlayerSpawnManager.cs`):
  - Singleton pattern for global spawn management
  - Manual and automatic spawn point detection
  - Persistent spawn position storage
  - DontDestroyOnLoad integration for scene transitions

## File Structure
- `playerScript.md` - Main first-person controller implementation
- `player/` - Player-related scripts (movement, animation, flashlight, spawning)
- `enemy/` - Enemy AI, catch mechanics, and behavior systems
- `environment/doors/` - Door interaction and animation systems
- `Pickups/` - Interactive pickup items and battery system
- `HUD/events/` - UI elements and event tracking
- `Idol/` - Jump scare and interactive horror elements
- `MainMenu/` - Menu system and scene management
- Various asset folders for models, materials, and scenes
