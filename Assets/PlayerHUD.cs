using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueEntry[] dialoguedb;
    public Text dialogueheader;
    public Text dialoguebody;
    public TimerRT dialogueadvancetimer;
    public Image dialoguecontinue;
    public AudioClipSoundControlCollection sfxtyping;
    public int sfxmod = 3;

    private DialogueEntry currententry = null;
    private string currentbodytext = "";
    private int currentbodytextidx = 0;
    private int currentsfxmod = 0;

    void Awake()
    {
        dialogueadvancetimer.Init();
        dialoguecontinue.gameObject.SetActive(false);
        ActivateEntry(0);
    }

    public void ActivateEntry(int idx)
    {
        if (currententry != null)
        {
            Debug.LogErrorFormat("Dialogue manager already has an active entry: {0}", currententry.name);
            return;
        }

        currententry = dialoguedb[idx];
        currentbodytext = "";
        currentbodytextidx = 0;
        dialoguebody.text = "";
    }

    void Update()
    {
        if(currententry != null)
        {
            if(dialoguecontinue.gameObject.activeInHierarchy)
            {
                // -- waiting for player input
                if(GameInput.FireTrigger())
                {
                    dialoguecontinue.gameObject.SetActive(false);
                    if(currententry.successorid != -1)
                    {
                        int id = currententry.successorid;
                        currententry = null;
                        ActivateEntry(id);
                    }
                    else
                        currententry = null;

                }
            }
            else
            {
                // -- advance dialogue
                dialogueadvancetimer.Tick(Time.deltaTime);
                if(dialogueadvancetimer.TimerReached())
                {
                    dialogueadvancetimer.Reset();

                    if(currentbodytextidx >= currententry.body.Length)
                    {
                        // -- we are finished
                        currentbodytext = currententry.body;
                        dialoguecontinue.gameObject.SetActive(true);
                    }
                    else
                    {
                        currentbodytext += currententry.body[currentbodytextidx++];
                    }

                    dialoguebody.text = currentbodytext;

                    ++currentsfxmod;
                    if(currentsfxmod % sfxmod == 0)
                        AudioManager.PlayRandomClip2D(sfxtyping.sounds);
                }
            }
        }
    }
}

[System.Serializable]
public class DialogueEntry
{
    public string name;
    public string header;
    [TextArea(1, 4)]
    public string body;
    public int successorid;
}