using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public Transform doorHinge;
    public float openAngle = 90f;
    public float speed = 2f;
    public bool debugMode = true;

    private bool isPlayerNear = false;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        if (doorHinge == null) doorHinge = transform;
        closedRotation = doorHinge.rotation;
        openRotation = doorHinge.rotation * Quaternion.Euler(0, openAngle, 0);
        
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

        doorHinge.rotation = Quaternion.Slerp(
            doorHinge.rotation,
            isOpen ? openRotation : closedRotation,
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
