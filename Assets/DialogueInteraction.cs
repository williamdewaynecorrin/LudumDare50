using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueInteraction : MonoBehaviour
{
    public GameObject interactionobject;
    public float interactionlerp = 6.0f;
    public string dialoguename;
    public bool disableafterfinish = false;
    public bool forceinteraction = false;
    private Vector3 interactionobjectscale;
    private bool activated = false;

    private PlayerHUD dialoguemanager;

    void Awake()
    {
        interactionobjectscale = interactionobject.transform.localScale;
        interactionobject.transform.localScale = Vector3.zero;
    }

    void Start()
    {
        dialoguemanager = GameObject.FindObjectOfType<PlayerHUD>();
    }

    void Update()
    {
        Vector3 targetscale = activated ? interactionobjectscale : Vector3.zero;
        interactionobject.transform.localScale = Vector3.Lerp(interactionobject.transform.localScale, targetscale, Time.deltaTime * interactionlerp);
    }

    public void BeginDialogue()
    {
        dialoguemanager.DialogueActivateEntry(dialoguename, this);
        if (disableafterfinish)
        {
            DeactivateInteraction();
            Collider[] allcoll = GetComponents<Collider>();
            foreach (Collider c in allcoll)
                c.enabled = false;
        }
    }

    public void OnDialogueEnd()
    {

    }

    public void ActivateInteraction()
    {
        activated = true;
    }

    public void DeactivateInteraction()
    {
        activated = false;
    }
}

public enum DialogueProgressionType
{
    Door = 0,
    Comrade = 1,
    Sequence = 2
}