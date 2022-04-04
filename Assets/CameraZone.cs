using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public Transform look;
    private PlayerCamera cam;

    void Start()
    {
        cam = GameObject.FindObjectOfType<PlayerCamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<PlayerController>() != null)
            cam.SetZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.GetComponent<PlayerController>() != null)
            cam.SetZone(null);
    }
}
