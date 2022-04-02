using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor3D : MonoBehaviour
{
    public LayerMask mask;

    private const float kBumpDistance = 0.001f;
    private const float kMaxRaycastDist = 650f;
    private new PlayerCamera camera;

    public Vector3 Position => transform.position;

    void Start()
    {
        camera = GameObject.FindObjectOfType<PlayerCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = camera.Camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, kMaxRaycastDist, mask, QueryTriggerInteraction.Collide))
        {
            transform.position = hit.point + hit.normal * kBumpDistance;
            transform.rotation = Quaternion.LookRotation(hit.normal);
        }
    }
}
