using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class jumpScareTrigger : MonoBehaviour
{
    [Header("Jump Scare Reference")]
    public JumpScare1 jumpScareObject; // Reference to the jump scare object
    
    [Header("Random Trigger Settings")]
    [Range(0f, 1f)]
    public float scareProbability = 0.5f; // Probability of triggering (0-1)
    public bool triggerOnce = true; // If true, only triggers once
    
    [Header("Optional Settings")]
    public string playerTag = "Player"; // Tag to check for collision
    public float cooldownTime = 5.0f; // Time before another scare can happen
    
    [Header("Debug Settings")]
    public bool debugMode = false; // Toggle debug features
    public Color triggerZoneColor = Color.red; // Color for gizmo visualization
    public bool showDebugLogs = true; // Show console logs when debug mode is on
    
    private bool hasTriggered = false;
    private bool coolingDown = false;
    private bool isPlayerNear = false; // Added player detection flag
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = true; // Set player detection flag to true
            
            if (debugMode && showDebugLogs)
            {
                Debug.Log($"[JumpScare] Player entered trigger zone: {gameObject.name}");
            }
            TryTriggerJumpScare();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = false; // Set player detection flag to false when player exits
            
            if (debugMode && showDebugLogs)
            {
                Debug.Log($"[JumpScare] Player exited trigger zone: {gameObject.name}");
            }
        }
    }
    
    // Draw visual representation of trigger area in editor
    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                Gizmos.color = hasTriggered ? Color.gray : triggerZoneColor;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
    }
    
    private void TryTriggerJumpScare()
    {
        // Only proceed if the player is near
        if (!isPlayerNear)
        {
            return;
        }
        
        // Check if we can trigger based on conditions
        if ((triggerOnce && hasTriggered) || coolingDown || jumpScareObject == null)
        {
            if (debugMode && showDebugLogs)
            {
                string reason = "";
                if (triggerOnce && hasTriggered) reason = "already triggered once";
                else if (coolingDown) reason = "in cooldown period";
                else if (jumpScareObject == null) reason = "no jump scare object assigned";
                
                Debug.Log($"[JumpScare] Trigger skipped ({reason}): {gameObject.name}");
            }
            return;
        }
        
        // Random chance to trigger
        float randomValue = Random.value;
        if (randomValue <= scareProbability)
        {
            if (debugMode && showDebugLogs)
            {
                Debug.Log($"[JumpScare] Triggered! Random value: {randomValue:F2}, Probability: {scareProbability:F2}");
            }
            
            jumpScareObject.TriggerJumpScare();
            hasTriggered = true;
            
            // Start cooldown if not triggering only once
            if (!triggerOnce)
            {
                StartCoroutine(Cooldown());
            }
        }
        else if (debugMode && showDebugLogs)
        {
            Debug.Log($"[JumpScare] Not triggered - Random check failed. Value: {randomValue:F2}, Threshold: {scareProbability:F2}");
        }
    }
    
    private IEnumerator Cooldown()
    {
        coolingDown = true;
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[JumpScare] Cooldown started for {cooldownTime} seconds: {gameObject.name}");
        }
        
        yield return new WaitForSeconds(cooldownTime);
        
        coolingDown = false;
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[JumpScare] Cooldown ended: {gameObject.name}");
        }
    }
    
    // Reset trigger for testing
    public void ResetTrigger()
    {
        hasTriggered = false;
        if (debugMode && showDebugLogs)
        {
            Debug.Log($"[JumpScare] Trigger reset: {gameObject.name}");
        }
    }
}
