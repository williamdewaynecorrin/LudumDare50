using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private const float kMaxLifetime = 7.0f;

    public Vector3 rotationaxis;
    public float rotateamount = 1.0f;

    private Vector3 velocity;

    void Awake()
    {
        rotationaxis.Normalize();
    }

    public void Initialize(Vector3 dir, float force, Vector3 additivevel)
    {
        velocity = dir * force + dir * additivevel.magnitude;

        GameObject.Destroy(this.gameObject, kMaxLifetime);
    }

    void FixedUpdate()
    {
        transform.position += velocity;
        transform.localRotation *= Quaternion.AngleAxis(rotateamount, rotationaxis);
    }

    void OnCollisionEnter(Collision c)
    {
        Enemy e = c.gameObject.GetComponent<Enemy>();
        if(e != null)
        {

        }

        GameObject.Destroy(this.gameObject);
    }
}
