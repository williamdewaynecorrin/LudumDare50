using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityComponent : MonoBehaviour
{
    private float force = 1.0f;
    private Vector3 direction = Vector3.down;

    public Vector3 Direction => direction;
    public float Force => force;
}
