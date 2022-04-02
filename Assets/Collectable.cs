using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Collectable : MonoBehaviour
{
    public CollectableType type = CollectableType.Trash;
    public int value = 20;
    public bool rotates = true;
    [ConditionalHide("rotates", true)]
    public float rotatespeed = 1.0f;
    public new SphereCollider collider;
    public GameObject graphics;

    void Update()
    {
        if(rotates)
        {
            transform.rotation *= Quaternion.AngleAxis(rotatespeed * Time.deltaTime, Vector3.up);
        }
    }

    public void Pickup()
    {
        collider.enabled = false;
        graphics.SetActive(false);

        GameObject.Destroy(this.gameObject);
    }
}

public enum CollectableType
{
    Trash = 0
}
