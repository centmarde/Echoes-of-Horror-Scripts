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

## File Structure
- `playerScript.md` - Main first-person controller implementation
- `showSpot.cs` - Additional utility script
- Various asset folders for models, materials, and scenes
