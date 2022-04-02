using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    public Vector3 offset;
    public float smooth = 1.0f;

    private PlayerController target;
    private Vector3 targetoffset;
    private new Camera camera;
    
    public Camera Camera => camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Start()
    {
        target = GameObject.FindObjectOfType<PlayerController>();
    }

    void FixedUpdate()
    {
        Vector3 targetpos = target.transform.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetpos, Time.fixedDeltaTime * smooth);
    }

    public Vector3 TargetForwardDirection()
    {
        return new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
    }

    public Vector3 TargetRightDirection()
    {
        return new Vector3(transform.right.x, 0f, transform.right.z).normalized;
    }
}
