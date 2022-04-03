using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameInput 
{
    public static float HorizontalInput()
    {
        float l = (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) ? -1f : 0f;
        float r = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) ? 1f : 0f;
        return l + r;
    }

    public static float VerticalInput()
    {
        float u = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) ? 1f : 0f;
        float d = (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) ? -1f : 0f;
        return u + d;
    }

    public static bool Fire()
    {
        return Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);
    }

    public static bool FireTrigger()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
    }
}
