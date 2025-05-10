using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScare1 : MonoBehaviour
{
    [Header("Jump Scare Settings")]
    public float moveDistance = 2.0f; // How far to move to the left
    public float moveSpeed = 10.0f; // How fast to move
    public AudioClip scareSound; // Sound to play
    public float returnDelay = 3.0f; // Time before returning to original position
    public string runAnimatorParameter = "isRun"; // Animator parameter name
    
    private Vector3 originalPosition;
    private AudioSource audioSource;
    private bool isMoving = false;
    private Animator animator;
    
    void Start()
    {
        originalPosition = transform.position;
        
        // Add AudioSource if none exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get the animator component
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        // Logic can be expanded here if needed
    }
    
    public void TriggerJumpScare()
    {
        if (!isMoving)
        {
            StartCoroutine(JumpScareSequence());
        }
    }
    
    private IEnumerator JumpScareSequence()
    {
        isMoving = true;
        
        // Set animator parameter to true
        if (animator != null)
        {
            animator.SetBool(runAnimatorParameter, true);
        }
        
        // Play sound
        if (scareSound != null)
        {
            audioSource.clip = scareSound;
            audioSource.Play();
        }
        
        // Move to the left quickly (negative X direction)
        Vector3 targetPosition = originalPosition + new Vector3(-moveDistance, 0, 0);
        float startTime = Time.time;
        float journeyLength = Vector3.Distance(originalPosition, targetPosition);
        
        while (Time.time < startTime + journeyLength / moveSpeed)
        {
            float distanceCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;
            transform.position = Vector3.Lerp(originalPosition, targetPosition, fractionOfJourney);
            yield return null;
        }
        
        transform.position = targetPosition;
        
        // Wait for sound to finish playing if it's playing
        if (scareSound != null && audioSource.isPlaying)
        {
            // Calculate remaining time on the audio clip
            float remainingTime = scareSound.length - audioSource.time;
            if (remainingTime > 0)
                yield return new WaitForSeconds(remainingTime);
        }
        
        // Hide the entire object
        gameObject.SetActive(false);
        
        // Wait before returning (keep the audio source playing if needed)
        yield return new WaitForSeconds(returnDelay);
        
        // Return to original position and show the object again
        transform.position = originalPosition;
        gameObject.SetActive(true);
        
        // Reset animator parameter
        if (animator != null)
        {
            animator.SetBool(runAnimatorParameter, false);
        }
        
        isMoving = false;
    }
}
