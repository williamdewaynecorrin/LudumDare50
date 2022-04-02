using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor3D : MonoBehaviour
{
    private const float kMaxRaycastDist = 100f;
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
        if (Physics.Raycast(ray, out RaycastHit hit, kMaxRaycastDist))
            transform.position = hit.point;
    }
}
