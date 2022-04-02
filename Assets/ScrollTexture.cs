using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class ScrollTexture : MonoBehaviour
{
    [SerializeField]
    private ScrollTextureData[] data;

    private new MeshRenderer renderer;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();

        for (int i = 0; i < data.Length; ++i)
        {
            data[i].material = renderer.materials[data[i].materialidx];
            data[i].texture = data[i].material.GetTexture(data[i].texturename);
            data[i].offset = data[i].material.GetTextureOffset(data[i].texturename);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < data.Length; ++i)
        {
            data[i].offset += data[i].scroll;
            data[i].material.SetTextureOffset(data[i].texturename, data[i].offset);
        }
    }

    public void SetScrollValue(int idx, Vector2 val)
    {
        data[idx].scroll = val;
    }
}

[System.Serializable]
public struct ScrollTextureData
{
    public string texturename;
    public Vector2 scroll;
    public int materialidx;

    [HideInInspector]
    public Material material;
    [HideInInspector]
    public Texture texture;
    [HideInInspector]
    public Vector2 offset;
}