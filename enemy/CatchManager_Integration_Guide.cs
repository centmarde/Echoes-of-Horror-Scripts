// CatchManager Integration Guide
// =============================
//
// This file contains setup instructions for integrating the CatchManager with enemyAi2 and FirstPersonController
//
// REQUIRED COMPONENTS:
// ===================
// 1. enemyAi2.cs - The main enemy AI script
// 2. CatchManager.cs - Handles catch detection and sequence
// 3. CatchCounter.cs - Already exists, tracks catches and resets game
// 4. checkSafe.cs - Already exists, handles safe zones (updated for enemyAi2)
// 5. EnemyCatchSetup.cs - Helper for auto-setup (optional but recommended)
//
// SETUP INSTRUCTIONS:
// ===================
//
// For Enemy GameObject:
// --------------------
// 1. Attach enemyAi2.cs component
// 2. Attach CatchManager.cs component  
// 3. Attach EnemyCatchSetup.cs component (optional, for auto-setup)
// 4. Configure CatchManager settings in inspector:
//    - Catch Range: 2.5 (distance at which catch occurs)
//    - Catch Sequence Duration: 3.0 (how long catch lasts)
//    - Player Freeze Time: 2.0 (how long player is frozen)
//    - Respawn Player On Catch: true (respawn after catch)
//    - Respawn Delay: 1.0 (delay before respawn)
//
// For Audio (Optional):
// --------------------
// 1. Add AudioSource component to enemy
// 2. Assign catch sound clip to CatchManager
// 3. Configure audio source settings
//
// For Visual Effects (Optional):
// -----------------------------
// 1. Create a UI Canvas with CanvasGroup for screen fade
// 2. Assign the CanvasGroup to CatchManager's screenFadeCanvas
// 3. Set fade duration as desired
//
// Player Setup:
// ------------
// 1. Ensure FirstPersonController.cs is attached to player
// 2. Player should have "Player" tag OR FirstPersonController component for auto-detection
// 3. Make sure CatchCounter component exists in scene (singleton)
//
// Safe Zones Setup:
// ----------------
// 1. Create GameObjects for safe zones
// 2. Add Colliders and set them as triggers
// 3. Tag them with "SafeZone"
// 4. Enemy will avoid chasing into safe zones and teleport away if they enter
//
// TESTING:
// ========
// 1. Run the scene
// 2. Let enemy detect and chase player
// 3. When enemy gets within catch range (2.5 units by default), catch sequence should trigger
// 4. Player should be frozen, catch sound plays, screen fades (if configured)
// 5. After sequence, player respawns and catch counter increments
// 6. When max catches reached (default 1), game resets
//
// DEBUGGING:
// ==========
// Enable "Show Debug Info" on CatchManager and enemyAi2 components for console output
// Use Scene view to see gizmos showing catch range, vision cones, and safe zones
//
// CUSTOMIZATION:
// ==============
// - Adjust catch range for easier/harder gameplay
// - Modify catch sequence duration for dramatic effect  
// - Change max catches in CatchCounter for multiple lives
// - Add multiple enemies with different catch behaviors
// - Implement different respawn locations
// - Add catch animations to enemy Animator
//
// PERFORMANCE NOTES:
// ==================
// - CatchManager checks distance every frame only when not in catch sequence
// - Uses trigger colliders as backup detection method
// - Safe zone checks use static methods for efficiency
// - Consider object pooling for multiple enemies

/*

EXAMPLE MONOBEHAVIOUR FOR TESTING:
==================================

using UnityEngine;

public class CatchManagerTester : MonoBehaviour
{
    [Header("Testing")]
    public KeyCode forceCatchKey = KeyCode.T;
    public KeyCode resetCounterKey = KeyCode.R;
    public KeyCode toggleCatchKey = KeyCode.C;
    
    private CatchManager catchManager;
    private CatchCounter catchCounter;
    
    private void Start()
    {
        catchManager = FindObjectOfType<CatchManager>();
        catchCounter = CatchCounter.Instance;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(forceCatchKey) && catchManager != null)
        {
            Debug.Log("Force triggering catch...");
            catchManager.ForceCatch();
        }
        
        if (Input.GetKeyDown(resetCounterKey) && catchCounter != null)
        {
            Debug.Log("Resetting catch counter...");
            catchCounter.ResetCounter();
        }
        
        if (Input.GetKeyDown(toggleCatchKey) && catchManager != null)
        {
            bool currentState = !catchManager.IsCatchSequenceActive();
            catchManager.SetCatchEnabled(currentState);
            Debug.Log($"Catch enabled: {currentState}");
        }
    }
}

*/
