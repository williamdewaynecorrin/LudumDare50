using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Dialogue")]
    public Font defaultfont;
    public DialogueEntry[] dialoguedb;
    public Transform dialoguebox;
    public Transform dialogueboxonscreen;
    public Transform dialogueboxoffscreen;
    public float dialogueanimlerp = 8.0f;
    public Text dialogueheader;
    public Text dialoguebody;
    public TimerRT dialogueadvancetimer;
    public Image dialoguecontinue;
    public AudioClipSoundControlCollection sfxtyping;
    public int sfxmod = 3;
    public AudioClipSoundControl sfxin;
    public AudioClipSoundControl sfxout; 
    public AudioClipSoundControl sfxcontinue;

    [Header("HUD")]
    public Image trashfill;
    public Text comradetext;
    public int maxcomrades = 5;

    private DialogueEntry currententry = null;
    private string currentbodytext = "";
    private int currentbodytextidx = 0;
    private int currentsfxmod = 0;
    private DialogueInteraction currentcomponent = null;
    private int comradecount = 0;

    void Start()
    {
        dialogueadvancetimer.Init();
        dialoguecontinue.gameObject.SetActive(false);
        dialoguebox.localPosition = dialogueboxoffscreen.localPosition;

        DialogueActivateEntry(0);
    }

    void Update()
    {
        if(currententry != null)
        {
            if (dialoguecontinue.gameObject.activeInHierarchy)
            {
                // -- waiting for player input
                if (GameInput.Interact())
                {
                    dialoguecontinue.gameObject.SetActive(false);
                    currententry.Complete(this);
                    if (currententry.successorid != -1)
                    {
                        int id = currententry.successorid;
                        currententry = null;
                        DialogueActivateEntry(id);
                        AudioManager.PlayClip2D(sfxcontinue);
                    }
                    else
                    {
                        currententry = null;
                        if (LevelManager.Paused)
                        {
                            LevelManager.SetPaused(false);
                            AudioManager.PlayClip2D(sfxout);
                        }
                    }
                }
            }
            else
            {
                if ((dialoguebox.localPosition - dialogueboxonscreen.localPosition).magnitude < 0.01f)
                {
                    // -- skipping
                    if (GameInput.Interact())
                    {
                        dialogueadvancetimer.Reset();
                        currentbodytext = currententry.body;
                        dialoguecontinue.gameObject.SetActive(true);
                        dialoguebody.text = currentbodytext;
                    }
                    else
                    {
                        // -- advance dialogue
                        dialogueadvancetimer.Tick(Time.deltaTime);
                        if (dialogueadvancetimer.TimerReached())
                        {
                            dialogueadvancetimer.Reset();

                            if (currentbodytextidx >= currententry.body.Length)
                            {
                                // -- we are finished
                                currentbodytext = currententry.body;
                                dialoguecontinue.gameObject.SetActive(true);
                            }
                            else
                            {
                                currentbodytext += currententry.body[currentbodytextidx++];

                                char cchar = currentbodytext[currentbodytext.Length - 1];
                                if (cchar != ' ' && cchar != '\n')
                                {
                                    // -- sfx
                                    ++currentsfxmod;
                                    if (currentsfxmod % sfxmod == 0)
                                        AudioManager.PlayRandomClip2D(sfxtyping.sounds);
                                }
                            }

                            dialoguebody.text = currentbodytext;
                        }
                    }
                }
            }

            dialoguebox.localPosition = Vector3.Lerp(dialoguebox.localPosition, dialogueboxonscreen.localPosition, dialogueanimlerp * Time.deltaTime);
        }
        else
        {
            dialoguebox.localPosition = Vector3.Lerp(dialoguebox.localPosition, dialogueboxoffscreen.localPosition, dialogueanimlerp * Time.deltaTime);
        }
    }

    // -- dialogue functions
    public void DialogueActivateEntry(int idx)
    {
        if (currententry != null)
        {
            Debug.LogErrorFormat("Dialogue manager already has an active entry: {0}", currententry.name);
            return;
        }

        if (!LevelManager.Paused)
        {
            LevelManager.SetPaused(true);
            AudioManager.PlayClip2D(sfxin);
        }

        currententry = dialoguedb[idx];
        currentbodytext = "";
        currentbodytextidx = 0;
        dialoguebody.text = "";
        dialogueheader.text = currententry.header;

        if (currententry.font == null)
        {
            dialoguebody.font = defaultfont;
        }
        else
        {
            dialoguebody.font = currententry.font;
        }
    }

    public void DialogueActivateEntry(string name, DialogueInteraction component)
    {
        for(int i = 0; i < dialoguedb.Length; ++i)
        {
            if (dialoguedb[i].name.Equals(name))
            {
                DialogueActivateEntry(i);
                currentcomponent = component;
                return;
            }
        }

        Debug.LogErrorFormat("Dialogue with name {0} not found", name);
    }

    // -- HUD functions
    public void HUDSetFill(float ratio)
    {
        trashfill.fillAmount = ratio;
    }

    public void HUDAddFoundComrade()
    {
        ++comradecount;
        comradetext.text = string.Format("{0}/{1}", comradecount, maxcomrades);
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
    public Font font;
    public DialogueProgressionType type = DialogueProgressionType.Sequence;
    public UnityEvent oncompletion;

    private bool completed = false;
    public void Complete(PlayerHUD manager)
    {
        if (!completed)
        {
            oncompletion?.Invoke();

            switch(type)
            {
                case DialogueProgressionType.Comrade:
                    manager.HUDAddFoundComrade();
                    break;
            }
        }

        completed = true;
    }
}