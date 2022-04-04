using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPauseable
{
    public float health = 100;
    public float movespeed = 0.05f;
    public float homeradius = 0.1f;
    public float distancetoattack = 5.0f;
    public float rotatelerp = 4.0f;
    public GameObject particledeath;
    public AudioClipSoundControl sfxdeath;

    private bool activated = false;
    private bool paused = false;
    private Vector3 home;
    private PlayerController player;
    private Vector3 lastmove;
    private new SphereCollider collider;

    void Start()
    {
        home = transform.position;
        player = GameObject.FindObjectOfType<PlayerController>();
        collider = GetComponent<SphereCollider>();
    }

    void Update()
    {
        if (paused)
            return;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lastmove, Vector3.up), Time.deltaTime * rotatelerp);
    }

    void FixedUpdate()
    {
        if (paused)
            return;

        // -- determine activation
        Vector3 toplayer = player.Center - transform.position;
        toplayer.y = 0;
        float d = toplayer.magnitude;
        activated = d <= distancetoattack;

        if (!activated)
        {
            // -- go home
            if (Vector3.Distance(transform.position, home) > homeradius)
            {
                Vector3 tohome = (home - transform.position).normalized;
                transform.position += tohome * movespeed;
                lastmove = tohome;
            }
            return;
        }

        // -- chase player
        Vector3 toplayerdir = toplayer.normalized;
        transform.position += toplayerdir * movespeed;
        lastmove = toplayerdir;
    }

    public void SetPaused(bool paused)
    {
        this.paused = paused;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if(health <= 0f)
        {
            GameObject.Destroy(GameObject.Instantiate(particledeath, transform.position + collider.center, Quaternion.identity), 4.0f);
            AudioManager.PlayClip3D(sfxdeath, transform.position);

            // -- no time i am hacking this in
            Enemy[] all = GameObject.FindObjectsOfType<Enemy>();
            if(all.Length == 1)
            {
                GameObject.Destroy(GameObject.FindObjectOfType<CameraZone>().gameObject);
                GameObject.FindObjectOfType<PlayerHUD>().DialogueActivateEntry(12);
            }

            GameObject.Destroy(this.gameObject);
        }
    }
}
