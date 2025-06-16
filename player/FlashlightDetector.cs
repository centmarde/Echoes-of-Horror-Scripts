using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Maximum detection distance for enemies")]
    public float maxDetectionDistance = 20f;
    
    [Tooltip("The flashlight cone angle in degrees")]
    public float detectionAngle = 30f;
    
    [Tooltip("Layer mask for enemy detection")]
    public LayerMask enemyLayer;
    
    [Tooltip("Layer mask for vision obstruction detection")]
    public LayerMask obstacleLayer;
    
    [Header("Debug")]
    public bool showDebugRays = false;
    
    // Internal references
    private flashlight attachedFlashlight;
    private Transform playerCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get reference to the attached flashlight component
        attachedFlashlight = GetComponent<flashlight>();
        
        if (attachedFlashlight == null)
        {
            attachedFlashlight = GetComponentInParent<flashlight>();
        }
        
        if (attachedFlashlight == null)
        {
            Debug.LogError("FlashlightDetector: No flashlight component found on this object or its parent.");
        }
        
        // Get reference to the main camera transform (for direction)
        if (Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }
        else
        {
            Debug.LogError("FlashlightDetector: No main camera found.");
        }
        
        // Set default enemy layer if not set
        if (enemyLayer == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy") != 0 ? LayerMask.GetMask("Enemy") : -1;
        }
        
        // Set default obstacle layer if not set
        if (obstacleLayer == 0)
        {
            obstacleLayer = LayerMask.GetMask("Default");
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        // Only detect enemies when the flashlight is on
        if (attachedFlashlight != null && attachedFlashlight.IsFlashlightOn())
        {
            DetectEnemiesInFlashlightCone();
        }
    }
    
    void DetectEnemiesInFlashlightCone()
    {
        // Use camera forward direction for detection
        Vector3 forwardDirection = playerCamera != null ? playerCamera.forward : transform.forward;
        Vector3 rayOrigin = transform.position;
        
        // Find all enemies in range
        Collider[] potentialEnemies = Physics.OverlapSphere(rayOrigin, maxDetectionDistance, enemyLayer);
        
        foreach (Collider enemyCollider in potentialEnemies)
        {
            // Calculate direction to the enemy
            Vector3 directionToEnemy = enemyCollider.transform.position - rayOrigin;
            float distanceToEnemy = directionToEnemy.magnitude;
            Vector3 directionToEnemyNormalized = directionToEnemy.normalized;
            
            // Check if enemy is within the cone angle
            float angle = Vector3.Angle(forwardDirection, directionToEnemyNormalized);
            
            if (angle <= detectionAngle * 0.5f)
            {
                // Cast a ray to check for obstacles between flashlight and enemy
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, directionToEnemyNormalized, out hit, distanceToEnemy, obstacleLayer | enemyLayer))
                {
                    if (showDebugRays)
                    {
                        Debug.DrawLine(rayOrigin, hit.point, hit.transform == enemyCollider.transform ? Color.green : Color.red);
                    }
                    
                    // Check if we hit the enemy first before any obstacle
                    if (hit.transform == enemyCollider.transform || hit.transform.IsChildOf(enemyCollider.transform))
                    {
                        // Try to get the EnemyController and call teleportation method
                        EnemyController enemyController = hit.transform.GetComponent<EnemyController>();
                        if (enemyController == null)
                        {
                            enemyController = hit.transform.GetComponentInParent<EnemyController>();
                        }
                        
                        if (enemyController != null && enemyController.canTeleportOnFlashlight)
                        {
                            // The CheckFlashlightExposure method in EnemyController will handle the rest
                            // It already contains the exposure timer and teleportation logic
                        }
                    }
                }
            }
        }
    }
    
    // Draw debug visualization gizmos in the editor
    private void OnDrawGizmosSelected()
    {
        if (showDebugRays)
        {
            Gizmos.color = Color.yellow;
            Vector3 forwardDirection = playerCamera != null ? playerCamera.forward : transform.forward;
            
            float halfAngle = detectionAngle * 0.5f * Mathf.Deg2Rad;
            float coneHeight = maxDetectionDistance;
            float coneRadius = Mathf.Tan(halfAngle) * coneHeight;
            
            Vector3 coneEnd = transform.position + forwardDirection * coneHeight;
            
            // Draw cone direction line
            Gizmos.DrawLine(transform.position, coneEnd);
            
            // Draw cone radius at the end
            Gizmos.DrawWireSphere(coneEnd, coneRadius);
        }
    }
}
