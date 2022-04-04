using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private static bool ispaused = false;
    public static bool Paused => ispaused;

    public static void SetPaused(bool paused)
    {
        ispaused = paused;

        IEnumerable<IPauseable> pausableobjects = FindObjectsOfType<MonoBehaviour>().OfType<IPauseable>();
        foreach (IPauseable s in pausableobjects)
        {
            s.SetPaused(ispaused);
        }
    }
}
