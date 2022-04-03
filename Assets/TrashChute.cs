using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashChute : MonoBehaviour
{
    public Collectable prefab;
    public Transform spawn;
    public float shootvelocity = 0.1f;
    public TimerRT spawntimer;

    private Collectable current;

    void Start()
    {
        spawntimer.Init();
        if(current == null)
        {
            Spawn();
        }
    }

    void Spawn()
    {
        current = GameObject.Instantiate(prefab, spawn.position, Quaternion.identity);
        current.Shoot(spawn.forward * shootvelocity);

        spawntimer.Reset();
    }

    void Update()
    {
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
}
