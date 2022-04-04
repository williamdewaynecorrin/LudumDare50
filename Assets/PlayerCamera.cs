using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    private const float kMaxRaycastDist = 100f;

    public Vector3 offset;
    public Vector3 backwardsoffset;
    public float smooth = 1.0f;
    [Range(0f, 1f)]
    public float lookblend;
    public float offsetlerp = 4.0f;
    public float predictionlerp = 4.0f;
    public LayerMask raycastmask;
    public LayerMask playermask;
    public Vector3 endperiod;
    public Vector3 endamp;
    public AudioClipSoundControl sfxend;
    public AudioClipSoundControl sfxend2;

    private PlayerController target;
    private Vector3 targetoffset;
    private new Camera camera;

    private float lastpredictionval = 1.0f;
    private Vector3 lasttargetpos;
    private Quaternion lasttargetrot;
    private Vector3 currentoffset;
    private CameraZone currentzone;

    private bool endofgame = false;
    private Vector3 endposition;
    private Quaternion endrotation;

    public Camera Camera => camera;

    void Awake()
    {
        camera = GetComponent<Camera>();
    }

    void Start()
    {
        target = GameObject.FindObjectOfType<PlayerController>();
        currentoffset = offset;
    }

    void FixedUpdate()
    {
        if(endofgame)
        {
            float endx = Mathf.Sin(Time.time * endperiod.x) * endamp.x;
            float endy = Mathf.Sin(Time.time * endperiod.y) * endamp.y;
            float endz = Mathf.Sin(Time.time * endperiod.z) * endamp.z;
            transform.position = endposition + new Vector3(endx, endy, endz);
        }
        else if(currentzone != null)
        {
            transform.position = Vector3.Lerp(transform.position, currentzone.look.position, Time.fixedDeltaTime * smooth);
            transform.rotation = Quaternion.Slerp(transform.rotation, currentzone.look.rotation, smooth * Time.fixedDeltaTime);
            return;
        }

        lastpredictionval = Mathf.Lerp(lastpredictionval, target.CameraPredictionSimilarity(), predictionlerp * Time.fixedDeltaTime);

        // -- calculate target transform attributes
        Vector3 calculatedoffset = Mathf.Abs(lastpredictionval) > 2.0f ? backwardsoffset : offset;
        currentoffset = Vector3.Lerp(currentoffset, calculatedoffset, Time.deltaTime * offsetlerp);

        Vector3 targetpos = target.transform.position + currentoffset + target.CameraPrediction(lastpredictionval * 2.0f);

        Vector3 totarget = (target.Center - transform.position).normalized;
        Quaternion targetrot = Quaternion.Slerp(Quaternion.LookRotation(Vector3.forward), 
                                                Quaternion.LookRotation(totarget), 
                                                lookblend);

        Vector3 desiredtotarget = (target.Center - targetpos);

        // -- raycast test for walls and such
        float playerdist = kMaxRaycastDist;
        if (Physics.Raycast(transform.position, totarget, out RaycastHit playerinfo, kMaxRaycastDist, playermask, QueryTriggerInteraction.Ignore))
        {
            playerdist = playerinfo.distance;
        }
        else
        {
            Debug.LogError("Player not found for raycast collision: this should never happen");
            playerdist = kMaxRaycastDist;
        }

        if (Physics.Raycast(targetpos, desiredtotarget.normalized, out RaycastHit objectinfo, desiredtotarget.magnitude, raycastmask, QueryTriggerInteraction.Ignore))
        {
            // -- something is in the way
            if (Physics.Raycast(target.Center, -totarget, out RaycastHit placementinfo, kMaxRaycastDist, raycastmask, QueryTriggerInteraction.Ignore))
            {
                lasttargetpos = placementinfo.point;
                //transform.position = lasttargetpos;
                lasttargetrot = targetrot;
            }
            else
                Debug.LogError("Error calculating object interference");
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

    public void SetZone(CameraZone zone)
    {
        currentzone = zone;
    }

    public void EndGame()
    {
        endofgame = true;
        endposition = transform.position;
        endrotation = transform.rotation;
        AudioManager.PlayClip2D(sfxend);
        AudioManager.PlayClip2D(sfxend2);

        StartCoroutine(EndGameRoutine());
    }

    private IEnumerator EndGameRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        Application.Quit();
    }
}
