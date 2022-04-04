using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorButton : MonoBehaviour
{
    public UnityEvent onbuttonpressed;
    public AudioClipSoundControl sfxbuttonpress;
    public Transform button;
    public Transform pressed;
    public float buttonlerp = 4.0f;
    public Material pressedmaterial;
    public List<DoorButton> sequencebuttons;

    private Vector3 unpressedpos;
    private bool buttonpressed;

    void Awake()
    {
        unpressedpos = button.position;
    }

    void Update()
    {
        Vector3 targetpos = buttonpressed ? pressed.position : unpressedpos;

        button.position = Vector3.Lerp(button.position, targetpos, Time.deltaTime * buttonlerp);
    }

    public void PressButton()
    {
        if(!buttonpressed)
        {
            if (sequencebuttons.Count == 0)
            {
                onbuttonpressed?.Invoke();
                button.GetComponent<MeshRenderer>().material = pressedmaterial;
            }
            else
            {
                button.GetComponent<MeshRenderer>().material = pressedmaterial;

                bool allpressed = true;
                foreach(DoorButton other in sequencebuttons)
                {
                    if (!other.buttonpressed)
                    {
                        allpressed = false;
                        break;
                    }
                }

                if(allpressed)
                {
                    onbuttonpressed?.Invoke();
                }
            }
        }

        buttonpressed = true;
        AudioManager.PlayClip3D(sfxbuttonpress, transform.position);
    }
}
