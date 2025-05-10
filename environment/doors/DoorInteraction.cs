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
        
        if (debugMode) Debug.Log("Door initialized. Use E key when near to open/close.");
        
        // Make sure there's a collider set as trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("Door is missing a collider! Add a collider and set it as trigger.");
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning("Door collider is not set as trigger! Enable 'Is Trigger' in the inspector.");
        }
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            isOpen = !isOpen;
            if (debugMode) Debug.Log("Door state changed to: " + (isOpen ? "Open" : "Closed"));
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
            if (debugMode) Debug.Log("Player entered door trigger zone");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (debugMode) Debug.Log("Player exited door trigger zone");
        }
    }
}
