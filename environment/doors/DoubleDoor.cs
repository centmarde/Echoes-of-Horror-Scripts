using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleDoor : MonoBehaviour
{
    public Transform leftDoorHinge;
    public Transform rightDoorHinge;
    public float openAngle = 90f;
    public float speed = 2f;
    public bool debugMode = true;

    private bool isPlayerNear = false;
    private bool isOpen = false;
    private Quaternion leftClosedRotation;
    private Quaternion rightClosedRotation;
    private Quaternion leftOpenRotation;
    private Quaternion rightOpenRotation;

    void Start()
    {
        if (leftDoorHinge == null || rightDoorHinge == null)
        {
            // No debug message
        }
        
        leftClosedRotation = leftDoorHinge.rotation;
        rightClosedRotation = rightDoorHinge.rotation;
        leftOpenRotation = leftDoorHinge.rotation * Quaternion.Euler(0, -openAngle, 0);
        rightOpenRotation = rightDoorHinge.rotation * Quaternion.Euler(0, openAngle, 0);
        
        // Make sure there's a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            // No debug message
        }
        else if (!col.isTrigger)
        {
            // No debug message
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
        }

        leftDoorHinge.rotation = Quaternion.Slerp(
            leftDoorHinge.rotation,
            isOpen ? leftOpenRotation : leftClosedRotation,
            Time.deltaTime * speed
        );

        rightDoorHinge.rotation = Quaternion.Slerp(
            rightDoorHinge.rotation,
            isOpen ? rightOpenRotation : rightClosedRotation,
            Time.deltaTime * speed
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}
