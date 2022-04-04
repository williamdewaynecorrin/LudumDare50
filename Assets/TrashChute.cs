using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashChute : MonoBehaviour, IPauseable
{
    public Collectable prefab;
    public Transform spawn;
    public float shootvelocity = 0.1f;
    public TimerRT spawntimer;
    public AudioClipSoundControlCollection sfxshoot;

    private Collectable current;
    private bool paused = false;

    void Start()
    {
        spawntimer.Init();
    }

    void Spawn(bool playsfx = true)
    {
        current = GameObject.Instantiate(prefab, spawn.position, Quaternion.identity);
        current.Shoot(spawn.forward * shootvelocity);

        spawntimer.Reset();

        if(playsfx)
            AudioManager.PlayRandomClip3D(sfxshoot.sounds, spawn.position);
    }

    void Update()
    {
        if (paused)
            return;

        if(spawntimer.TimerReached())
        {
            if(current == null)
            {
                Spawn();
            }
        }
        else
            spawntimer.Tick(Time.deltaTime);
    }

    public void SetPaused(bool paused)
    {
        this.paused = paused;
    }
}
