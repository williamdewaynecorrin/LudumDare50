using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public Transform openedtransform;
    public AudioClipSoundControl sfxopen;
    private bool opened = false;

    void Start()
    {
        openedtransform.SetParent(null);
    }

    void Update()
    {
        
    }

    public void ActivateDoor()
    {
        AudioManager.PlayClip3D(sfxopen, transform.position);
        StartCoroutine(ActivateDoorAnimation());
    }

    private IEnumerator ActivateDoorAnimation(float frames = 100)
    {
        Vector3 target = openedtransform.position;
        Vector3 step = (target - transform.position) / frames;

        for(int i = 0; i < (int)frames; ++i)
        {
            transform.position += step;
            yield return new WaitForFixedUpdate();
        }

        transform.position = target;
        GameObject.Destroy(this.gameObject);
    }
}
