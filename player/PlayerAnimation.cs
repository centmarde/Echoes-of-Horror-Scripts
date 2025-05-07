using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    private readonly int speedHash = Animator.StringToHash("speed");
    private readonly int walkHash = Animator.StringToHash("walk");
    private readonly int runHash = Animator.StringToHash("run");
    private readonly string playerTag = "Player";
    
    private void Awake()
    {
        // Get animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Set the gameObject tag to "Player"
        gameObject.tag = playerTag;
    }
    
    private void Update()
    {
        HandleMovementInput();
    }
    
    private void HandleMovementInput()
    {
        // Check if any movement key is pressed (WASD)
        bool isMoving = Input.GetKey(KeyCode.W) || 
                        Input.GetKey(KeyCode.A) || 
                        Input.GetKey(KeyCode.S) || 
                        Input.GetKey(KeyCode.D);
        
        // Check if shift is being held for running
        bool isRunning = isMoving && Input.GetKey(KeyCode.LeftShift);
        
        // Set the animation parameters
        animator.SetBool(walkHash, isMoving && !isRunning);
        animator.SetBool(runHash, isRunning);
    }
    
    public void UpdateSpeed(float speed)
    {
        // Simply update the speed parameter in the animator
        animator.SetFloat(speedHash, speed);
    }
}
