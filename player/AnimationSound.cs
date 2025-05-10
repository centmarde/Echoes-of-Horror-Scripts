using UnityEngine;

public class AnimationSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip textingSound;
    [SerializeField] private float textingVolume = 2.0f;

    private void Awake()
    {
        // If no audio source is assigned, try to get one from the game object
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            
            // If there's still no audio source, add one
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    // This function will be called by the 'Texting' animation event
    public void PlayTextingSound()
    {
        if (audioSource != null && textingSound != null)
        {
            audioSource.PlayOneShot(textingSound, textingVolume);
        }
        else
        {
            Debug.LogWarning("Texting sound or audio source not assigned in AnimationSound script.");
        }
    }
}
