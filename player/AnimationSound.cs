using UnityEngine;

public class AnimationSound : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] textingSounds; // Changed to array
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
        if (audioSource != null && textingSounds != null && textingSounds.Length > 0)
        {
            // Pick a random sound from the array
            AudioClip randomSound = GetRandomTextingSound();
            if (randomSound != null)
            {
                audioSource.PlayOneShot(randomSound, textingVolume);
            }
        }
        else
        {
            Debug.LogWarning("Texting sounds or audio source not assigned in AnimationSound script.");
        }
    }

    // Get a random texting sound from the array
    private AudioClip GetRandomTextingSound()
    {
        if (textingSounds == null || textingSounds.Length == 0)
            return null;

        int randomIndex = Random.Range(0, textingSounds.Length);
        return textingSounds[randomIndex];
    }
}
