using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    private const float kMaxRaycastDist = 100f;

    public Vector3 offset;
    public float smooth = 1.0f;
    [Range(0f, 1f)]
    public float lookblend;
    public float predictionlerp = 4.0f;
    public LayerMask raycastmask;
    public LayerMask playermask;

    private PlayerController target;
    private Vector3 targetoffset;
    private new Camera camera;

    private float lastpredictionval = 1.0f;
    private Vector3 lasttargetpos;
    private Quaternion lasttargetrot;

    public Camera Camera => camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Start()
    {
        target = GameObject.FindObjectOfType<PlayerController>();
    }

    void FixedUpdate()
    {
        lastpredictionval = Mathf.Lerp(lastpredictionval, target.CameraPredictionSimilarity(), predictionlerp * Time.fixedDeltaTime);

        // -- calculate target transform attributes
        Vector3 targetpos = target.transform.position + offset + target.CameraPrediction(lastpredictionval * 2.0f);

        Vector3 totarget = (target.Center - transform.position).normalized;
        Quaternion targetrot = Quaternion.Slerp(Quaternion.LookRotation(Vector3.forward), 
                                                Quaternion.LookRotation(totarget), 
                                                lookblend);

        // -- raycast test for walls and such
        float playerdist = kMaxRaycastDist;
        if (Physics.Raycast(transform.position, totarget, out RaycastHit playerinfo, kMaxRaycastDist, playermask, QueryTriggerInteraction.Ignore))
        {
            playerdist = playerinfo.distance;
        }
        else
            Debug.LogError("Player not found for raycast collision: this should never happen");

        if (Physics.Raycast(transform.position, totarget, out RaycastHit objectinfo, playerdist, raycastmask, QueryTriggerInteraction.Ignore))
        {
            lasttargetpos = transform.position;
            lasttargetrot = transform.rotation;
        }
        else
        {
            lasttargetpos = targetpos;
            lasttargetrot = targetrot;
        }

        transform.position = Vector3.Lerp(transform.position, lasttargetpos, Time.fixedDeltaTime * smooth);
        transform.rotation = Quaternion.Slerp(transform.rotation, lasttargetrot, smooth * Time.fixedDeltaTime);
    }

    public Vector3 TargetForwardDirection()
    {
        return new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
    }

    public Vector3 TargetRightDirection()
    {
        return new Vector3(transform.right.x, 0f, transform.right.z).normalized;
    }
}
