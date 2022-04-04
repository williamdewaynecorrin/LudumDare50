using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Collectable : MonoBehaviour, IPauseable
{
    public CollectableType type = CollectableType.Trash;
    public int value = 20;
    public bool rotates = true;
    [ConditionalHide("rotates", true)]
    public float rotatespeed = 1.0f;
    public new SphereCollider collider;
    public GameObject graphics;
    public AudioClipSoundControl sfxpickup;

    private new Rigidbody rigidbody;
    private bool paused = false;
    private bool rigidbodywaskinematic;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 velocity)
    {
        rigidbody.velocity = velocity;
    }

    void Update()
    {
        if (paused)
            return;

        if(rotates)
        {
            transform.rotation *= Quaternion.AngleAxis(rotatespeed * Time.deltaTime, Vector3.up);
        }
    }

    public void Pickup()
    {
        collider.enabled = false;
        graphics.SetActive(false);
        AudioManager.PlayClip2D(sfxpickup);

        GameObject.Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision c)
    {
        if (paused)
            return;

        rigidbody.isKinematic = true;
        collider.isTrigger = true;
    }

    public void SetPaused(bool paused)
    {
        this.paused = paused;
        if (paused)
        {
            rigidbodywaskinematic = rigidbody.isKinematic;
            rigidbody.isKinematic = false;
        }
        else
            rigidbody.isKinematic = rigidbodywaskinematic;
    }
}

public enum CollectableType
{
    Trash = 0
}
