using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wiggle : MonoBehaviour
{
    public Vector3 period;
    public Vector3 amplitude;

    private Quaternion startrot;

    void Awake()
    {
        startrot = transform.localRotation;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * period.x) * amplitude.x;
        float y = Mathf.Cos(Time.time * period.y) * amplitude.y;
        float z = -Mathf.Sin(Time.time * period.z) * amplitude.z;

        Quaternion wigglerot = Quaternion.Euler(x, y, z);
        transform.localRotation = startrot * wigglerot;
    }
}
