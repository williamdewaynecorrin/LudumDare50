using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingUI : MonoBehaviour
{
    public Image trashfill;

    private PlayerController target;
    private Vector3 targetoffset;


    public void SetTarget(PlayerController target)
    {
        this.target = target;
        targetoffset = transform.position - target.transform.position;
        transform.SetParent(null);
    }

    public void SetFill(float ratio)
    {
        trashfill.fillAmount = ratio;
    }

    void Update()
    {
        transform.position = target.transform.position + targetoffset;
    }
}
