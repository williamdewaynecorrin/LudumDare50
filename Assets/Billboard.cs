using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    public float smoothing = 6.0f;
    private new PlayerCamera camera;

    void Start()
    {
        camera = GameObject.FindObjectOfType<PlayerCamera>();
    }

    void Update()
    {
        Quaternion targetrot = camera.transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetrot, Time.deltaTime * smoothing);
    }
}
