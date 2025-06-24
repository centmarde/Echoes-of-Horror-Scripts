using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyWatchingSettings
{
    [Header("Enemy Reference")]
    public GameObject enemyGameObject;
    public enemyAi2 enemyAI;
    
    [Header("Watching Behavior")]
    [Tooltip("Should this enemy freeze completely when watched?")]
    public bool freezeWhenWatched = true;
    
    [Tooltip("Speed multiplier when watched (if not freezing completely)")]
    [Range(0f, 1f)]
    public float watchedSpeedMultiplier = 0.1f;
    
    [Tooltip("How long player must look away before enemy unfreezes")]
    public float unfreezeDelay = 0.5f;
    
    [Header("Advanced Settings")]
    [Tooltip("Should the enemy be aware it's being watched? (affects behavior)")]
    public bool enemyAwareness = true;
    
    [Tooltip("Distance threshold for watching behavior")]
    public float watchingDistance = 15f;
}

public class EnemyWatchingManager : MonoBehaviour
{
    [Header("Global Settings")]
    [Tooltip("Reference to the player watching detector")]
    public PlayerWatchingDetector watchingDetector;
    
    [Tooltip("List of enemies and their watching settings")]
    public List<EnemyWatchingSettings> enemies = new List<EnemyWatchingSettings>();
    
    [Header("Auto-Discovery")]
    [Tooltip("Automatically find and add all enemyAi2 components in the scene")]
    public bool autoDiscoverEnemies = true;
    
    [Tooltip("Default settings for auto-discovered enemies")]
    public EnemyWatchingSettings defaultSettings = new EnemyWatchingSettings();
    
    [Header("Audio Feedback")]
    [Tooltip("Sound to play when an enemy freezes")]
    public AudioClip freezeSound;
    
    [Tooltip("Sound to play when an enemy unfreezes")]
    public AudioClip unfreezeSound;
    
    [Tooltip("Audio source for playing sounds")]
    public AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    [SerializeField] private int currentlyWatchedEnemies = 0;
    [SerializeField] private int totalFrozenEnemies = 0;
    
    private Dictionary<GameObject, float> unfreezeTimers = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> previouslyWatched = new Dictionary<GameObject, bool>();
    
    private void Start()
    {
        // Find watching detector if not assigned
        if (watchingDetector == null)
        {
            watchingDetector = FindObjectOfType<PlayerWatchingDetector>();
            if (watchingDetector == null)
            {
                Debug.LogWarning("EnemyWatchingManager: No PlayerWatchingDetector found in scene!");
            }
        }
        
        // Auto-discover enemies if enabled
        if (autoDiscoverEnemies)
        {
            DiscoverEnemies();
        }
        
        // Validate enemy references
        ValidateEnemyReferences();
        
        // Get audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    private void Update()
    {
        if (watchingDetector == null) return;
        
        currentlyWatchedEnemies = 0;
        totalFrozenEnemies = 0;
        
        // Update each enemy's watching state
        foreach (var enemySettings in enemies)
        {
            if (enemySettings.enemyGameObject == null || enemySettings.enemyAI == null)
                continue;
                
            UpdateEnemyWatchingState(enemySettings);
        }
        
        // Update unfreeze timers
        UpdateUnfreezeTimers();
    }
    
    private void UpdateEnemyWatchingState(EnemyWatchingSettings settings)
    {
        GameObject enemy = settings.enemyGameObject;
        bool isBeingWatched = watchingDetector.IsWatchingEnemy(enemy);
        
        // Check distance threshold
        if (watchingDetector.playerCamera != null)
        {
            float distance = Vector3.Distance(enemy.transform.position, watchingDetector.playerCamera.transform.position);
            if (distance > settings.watchingDistance)
            {
                isBeingWatched = false;
            }
        }
        
        // Track statistics
        if (isBeingWatched) currentlyWatchedEnemies++;
        if (settings.enemyAI.IsBeingWatched()) totalFrozenEnemies++;
        
        // Check if watching state changed
        bool wasWatched = previouslyWatched.ContainsKey(enemy) ? previouslyWatched[enemy] : false;
        previouslyWatched[enemy] = isBeingWatched;
        
        if (isBeingWatched)
        {
            // Player is watching this enemy
            if (settings.freezeWhenWatched)
            {
                // Apply complete freeze
                settings.enemyAI.completelyStopWhenWatched = true;
            }
            else
            {
                // Apply speed reduction
                settings.enemyAI.completelyStopWhenWatched = false;
                settings.enemyAI.watchedSpeedMultiplier = settings.watchedSpeedMultiplier;
            }
            
            // Reset unfreeze timer
            if (unfreezeTimers.ContainsKey(enemy))
            {
                unfreezeTimers[enemy] = 0f;
            }
            
            // Play freeze sound if just started watching
            if (!wasWatched && freezeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(freezeSound);
            }
            
            if (showDebugInfo && !wasWatched)
            {
                Debug.Log($"Enemy {enemy.name} is now being watched and will freeze/slow down");
            }
        }
        else if (wasWatched)
        {
            // Player stopped watching - start unfreeze timer
            if (!unfreezeTimers.ContainsKey(enemy))
            {
                unfreezeTimers[enemy] = 0f;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Player stopped watching {enemy.name}, starting unfreeze timer");
            }
        }
    }
      private void UpdateUnfreezeTimers()
    {
        List<GameObject> toRemove = new List<GameObject>();
        List<GameObject> keysToUpdate = new List<GameObject>(unfreezeTimers.Keys);
        
        foreach (GameObject enemy in keysToUpdate)
        {
            if (!unfreezeTimers.ContainsKey(enemy))
                continue;
                
            float timer = unfreezeTimers[enemy];
            
            if (enemy == null)
            {
                toRemove.Add(enemy);
                continue;
            }
            
            // Find enemy settings
            EnemyWatchingSettings settings = null;
            foreach (var enemySettings in enemies)
            {
                if (enemySettings.enemyGameObject == enemy)
                {
                    settings = enemySettings;
                    break;
                }
            }
            
            if (settings == null)
            {
                toRemove.Add(enemy);
                continue;
            }
            
            // Update timer
            timer += Time.deltaTime;
            unfreezeTimers[enemy] = timer;
            
            // Check if unfreeze delay has passed
            if (timer >= settings.unfreezeDelay)
            {
                // Unfreeze the enemy
                if (settings.enemyAI != null)
                {
                    settings.enemyAI.freezeGracePeriod = 0f; // Override grace period
                }
                
                // Play unfreeze sound
                if (unfreezeSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(unfreezeSound);
                }
                
                toRemove.Add(enemy);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Enemy {enemy.name} unfrozen after grace period");
                }
            }
        }
          // Remove completed timers
        foreach (var enemy in toRemove)
        {
            unfreezeTimers.Remove(enemy);
        }
    }
    
    private void DiscoverEnemies()
    {
        enemyAi2[] allEnemies = FindObjectsOfType<enemyAi2>();
        
        foreach (var enemy in allEnemies)
        {
            // Check if already in list
            bool alreadyAdded = false;
            foreach (var existing in enemies)
            {
                if (existing.enemyGameObject == enemy.gameObject)
                {
                    alreadyAdded = true;
                    break;
                }
            }
            
            if (!alreadyAdded)
            {
                // Create new settings based on defaults
                EnemyWatchingSettings newSettings = new EnemyWatchingSettings
                {
                    enemyGameObject = enemy.gameObject,
                    enemyAI = enemy,
                    freezeWhenWatched = defaultSettings.freezeWhenWatched,
                    watchedSpeedMultiplier = defaultSettings.watchedSpeedMultiplier,
                    unfreezeDelay = defaultSettings.unfreezeDelay,
                    enemyAwareness = defaultSettings.enemyAwareness,
                    watchingDistance = defaultSettings.watchingDistance
                };
                
                enemies.Add(newSettings);
                
                if (showDebugInfo)
                {
                    Debug.Log($"Auto-discovered enemy: {enemy.name}");
                }
            }
        }
    }
    
    private void ValidateEnemyReferences()
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var settings = enemies[i];
            
            if (settings.enemyGameObject == null)
            {
                enemies.RemoveAt(i);
                continue;
            }
            
            if (settings.enemyAI == null)
            {
                settings.enemyAI = settings.enemyGameObject.GetComponent<enemyAi2>();
                if (settings.enemyAI == null)
                {
                    Debug.LogWarning($"Enemy {settings.enemyGameObject.name} does not have enemyAi2 component!");
                    enemies.RemoveAt(i);
                }
            }
        }
    }
    
    // Public methods for external control
    public void FreezeAllEnemies()
    {
        foreach (var settings in enemies)
        {
            if (settings.enemyAI != null)
            {
                settings.enemyAI.ForceFreeze(true);
            }
        }
    }
    
    public void UnfreezeAllEnemies()
    {
        foreach (var settings in enemies)
        {
            if (settings.enemyAI != null)
            {
                settings.enemyAI.ForceFreeze(false);
            }
        }
        
        unfreezeTimers.Clear();
    }
    
    public void AddEnemy(GameObject enemy)
    {
        enemyAi2 ai = enemy.GetComponent<enemyAi2>();
        if (ai == null)
        {
            Debug.LogWarning($"Cannot add {enemy.name} - no enemyAi2 component found");
            return;
        }
        
        // Check if already added
        foreach (var existing in enemies)
        {
            if (existing.enemyGameObject == enemy)
            {
                Debug.LogWarning($"Enemy {enemy.name} is already in the watching manager");
                return;
            }
        }
        
        // Add with default settings
        EnemyWatchingSettings newSettings = new EnemyWatchingSettings
        {
            enemyGameObject = enemy,
            enemyAI = ai,
            freezeWhenWatched = defaultSettings.freezeWhenWatched,
            watchedSpeedMultiplier = defaultSettings.watchedSpeedMultiplier,
            unfreezeDelay = defaultSettings.unfreezeDelay,
            enemyAwareness = defaultSettings.enemyAwareness,
            watchingDistance = defaultSettings.watchingDistance
        };
        
        enemies.Add(newSettings);
    }
    
    public void RemoveEnemy(GameObject enemy)
    {
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i].enemyGameObject == enemy)
            {
                enemies.RemoveAt(i);
                break;
            }
        }
        
        if (unfreezeTimers.ContainsKey(enemy))
        {
            unfreezeTimers.Remove(enemy);
        }
        
        if (previouslyWatched.ContainsKey(enemy))
        {
            previouslyWatched.Remove(enemy);
        }
    }
    
    // Getter methods for debugging/UI
    public int GetWatchedEnemyCount() => currentlyWatchedEnemies;
    public int GetFrozenEnemyCount() => totalFrozenEnemies;
    public int GetTotalEnemyCount() => enemies.Count;
}
