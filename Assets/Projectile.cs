using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    private const float kMaxLifetime = 7.0f;

    public Vector3 rotationaxis;
    public float rotateamount = 1.0f;
    public AudioClipSoundControlCollection sfxcollide;
    public GameObject particleimpact;

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
            e.TakeDamage(25f);
        }

        DoorButton b = c.gameObject.GetComponent<DoorButton>();
        if(b != null)
        {
            b.PressButton();
        }

        GameObject impactparticles = GameObject.Instantiate(particleimpact, c.contacts[0].point + c.contacts[0].normal * 0.001f, Quaternion.LookRotation(c.contacts[0].normal));
        GameObject.Destroy(impactparticles, 2.0f);

        AudioManager.PlayRandomClip3D(sfxcollide.sounds, transform.position);

        GameObject.Destroy(this.gameObject);
    }
}
