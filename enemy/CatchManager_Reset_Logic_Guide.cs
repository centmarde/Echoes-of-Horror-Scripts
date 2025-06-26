// CatchManager with PlayerFreezeController Reset Logic
// ===================================================
//
// UPDATED: CatchManager now uses the comprehensive reset logic from PlayerFreezeController
// instead of relying on PlayerSpawnManager. This makes the system more self-contained.
//
// KEY IMPROVEMENTS:
// =================
//
// 1. SELF-CONTAINED RESPAWN SYSTEM:
//    ✅ No longer depends on PlayerSpawnManager
//    ✅ Handles CharacterController properly during teleport
//    ✅ Multiple fallback respawn options
//    ✅ Stores original enemy position for reset
//
// 2. ENHANCED ENEMY RESET:
//    ✅ Properly disables/enables enemy AI during sequence
//    ✅ Freezes enemy rigidbody during catch
//    ✅ Resets enemy to original spawn position
//    ✅ Clears animation states properly
//
// 3. IMPROVED PLAYER HANDLING:
//    ✅ Disables CharacterController during teleport (prevents collision issues)
//    ✅ Resets player velocity and rotation
//    ✅ Multiple respawn position sources (respawnPoint, PlayerSpawnManager, initial position)
//
// 4. BETTER INTEGRATION:
//    ✅ Works with both enemyAi2 and existing PlayerSpawnManager (if present)
//    ✅ Maintains compatibility with existing systems
//    ✅ Enhanced debug logging
//
// RESPAWN PRIORITY ORDER:
// =======================
//
// 1. PlayerSpawnManager.Instance (if exists)
// 2. respawnPoint Transform (if assigned)
// 3. playerRespawnPosition (initial player position)
//
// NEW FEATURES ADDED:
// ===================
//
// SetRespawnPoint(Transform) - Set respawn using Transform reference
// originalEnemyPosition - Enemy returns to spawn location after catch
// Enhanced CharacterController handling
// Proper rigidbody velocity reset
// Improved animation state management
//
// USAGE EXAMPLES:
// ===============

using UnityEngine;

public class CatchManagerResetSetupExample : MonoBehaviour
{
    [Header("Setup References")]
    public GameObject enemy;
    public Transform playerSpawnPoint;
    
    void Start()
    {
        SetupAdvancedCatchSystem();
    }
    
    void SetupAdvancedCatchSystem()
    {
        if (enemy == null) return;
        
        // Get or add CatchManager
        CatchManager catchManager = enemy.GetComponent<CatchManager>();
        if (catchManager == null)
        {
            catchManager = enemy.AddComponent<CatchManager>();
        }
        
        // Configure respawn settings
        if (playerSpawnPoint != null)
        {
            catchManager.SetRespawnPoint(playerSpawnPoint);
        }
        
        // Configure catch behavior
        catchManager.catchRange = 2.5f;
        catchManager.respawnPlayerOnCatch = true;
        catchManager.respawnDelay = 2f; // Time for dramatic effect
        
        // Configure cinematic sequence
        catchManager.shakeDuration = 1.5f;
        catchManager.playerLiftDuration = 3f;
        catchManager.monsterPlayerDistance = 2f;
        
        Debug.Log("Advanced catch system configured!");
    }
}

// TESTING THE SYSTEM:
// ===================

/*
public class CatchSystemResetTester : MonoBehaviour
{
    [Header("Test Controls")]
    public KeyCode testCatchKey = KeyCode.T;
    public KeyCode setRespawnKey = KeyCode.R;
    public KeyCode movePlayerKey = KeyCode.M;
    
    private CatchManager catchManager;
    private FirstPersonController player;
    
    void Start()
    {
        catchManager = FindObjectOfType<CatchManager>();
        player = FindObjectOfType<FirstPersonController>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(testCatchKey) && catchManager != null)
        {
            Debug.Log("Testing catch sequence...");
            catchManager.ForceCatch();
        }
        
        if (Input.GetKeyDown(setRespawnKey) && catchManager != null)
        {
            Vector3 newRespawn = transform.position;
            catchManager.SetRespawnPosition(newRespawn);
            Debug.Log($"Set new respawn position: {newRespawn}");
        }
        
        if (Input.GetKeyDown(movePlayerKey) && player != null)
        {
            player.transform.position += Vector3.forward * 5f;
            Debug.Log("Moved player forward");
        }
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 150));
        GUILayout.Label("Catch System Tester");
        GUILayout.Label($"{testCatchKey} - Force Catch");
        GUILayout.Label($"{setRespawnKey} - Set Respawn Here");
        GUILayout.Label($"{movePlayerKey} - Move Player");
        
        if (catchManager != null)
        {
            GUILayout.Label($"Catch Active: {catchManager.IsCatchSequenceActive()}");
        }
        GUILayout.EndArea();
    }
}
*/

// MIGRATION FROM PlayerFreezeController:
// ======================================
//
// If you were using PlayerFreezeController with enemyAi:
// 1. Replace PlayerFreezeController with CatchManager
// 2. Change enemyAi requirement to enemyAi2
// 3. All respawn logic transfers directly
// 4. You get advanced catch sequence for free!
//
// Settings mapping:
// PlayerFreezeController → CatchManager
// freezeDistance → catchRange
// resetDelay → respawnDelay
// respawnPoint → respawnPoint (same)
// resetPlayerOnCatch → respawnPlayerOnCatch
//
// PERFORMANCE NOTES:
// ==================
//
// ✅ More efficient than PlayerSpawnManager dependency
// ✅ Reduces component coupling
// ✅ Handles edge cases better (CharacterController, etc.)
// ✅ Self-contained system with better error handling
// ✅ Proper cleanup and state management
