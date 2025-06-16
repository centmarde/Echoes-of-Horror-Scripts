using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportEffect : MonoBehaviour
{
    [Header("Visual Settings")]
    [Tooltip("Particle system for teleport effect")]
    public ParticleSystem teleportParticles;
    
    [Tooltip("How long the effect should last before destroying itself")]
    public float effectDuration = 2f;
    
    [Tooltip("Audio clip to play when teleporting")]
    public AudioClip teleportSound;
    
    [Tooltip("Volume for the teleport sound")]
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;
    
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        // Start self-destruction timer
        Destroy(gameObject, effectDuration);
        
        // Play particle system if available
        if (teleportParticles != null)
        {
            teleportParticles.Play();
        }
        else
        {
            // Create default particle effect if none is assigned
            CreateDefaultEffect();
        }
        
        // Play sound effect if available
        if (teleportSound != null)
        {
            // Create audio source if needed
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // Make sound 3D
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 30f;
            }
            
            audioSource.clip = teleportSound;
            audioSource.volume = soundVolume;
            audioSource.Play();
        }
    }
    
    // Create a simple default particle effect
    private void CreateDefaultEffect()
    {
        // Create particle system
        GameObject particleObj = new GameObject("DefaultTeleportParticles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        ParticleSystem particles = particleObj.AddComponent<ParticleSystem>();
        teleportParticles = particles;
        
        // Configure basic particle system
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 1f;
        main.startSize = 0.5f;
        main.startSpeed = 2f;
        main.startColor = new Color(0.2f, 0.6f, 1f);
        main.duration = 1f;
        main.loop = false;
        
        // Shape module
        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        // Emission
        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 50) });
        
        // Start the effect
        particles.Play();
    }
}