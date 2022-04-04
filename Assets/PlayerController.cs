using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IPauseable
{
    [Header("Movement")]
    public float acceleration = 0.04f;
    public float maxspeed = 0.5f;
    public float stopdamp = 0.95f;

    [Header("Physics")]
    public new SphereCollider collider;
    public float spherecastdist = 1.0f;
    public float spherecastforwarddist = 1.0f;
    public float raycastdist = 5.0f;
    public GravityComponent gravity;
    public LayerMask ground;
    public float anglethreshold = 45f;

    [Header("Rotations")]
    public Transform head;
    public Vector3 headorientation;
    public float headsmoothing = 6.0f;
    public float rotationsmoothing = 1.0f;
    public Vector3 normaltofacing;

    [Header("Treads")]
    public ScrollTexture[] treads;
    public float treadspeed = 1.0f;

    [Header("Shooting")]
    public TimerRT shootcooldown;
    public int maxtrash = 100;
    public int trashballcost = 5;
    public float firespeed = 1.0f;
    public Projectile trashballprefab;
    public Transform firetransform;

    [Header("UI")]
    public FloatingUI floatingui;

    [Header("Audio")]
    public SmartAudio treadsaudio;
    public AudioClip sfxtreadsmove;
    [Range(0f,1f)]
    public float treadsaudiomaxvolume = 0.7f;
    public float treadsaudiominpitch = 0.8f;
    public float treadsaudiomaxpitch = 1.8f;
    public AudioClipSoundControlCollection sfxfire;

    [Header("FX")]
    public Timer treadsmoketimer;
    public Transform treadsmokeparticleemitter;
    public GameObject particlestreadsmoke;
    public GameObject particlesdeath;

    private new PlayerCamera camera;
    private new Rigidbody rigidbody;
    private Cursor3D cursor;
    private PlayerHUD playerhud;
    private DialogueInteraction currentdialogue;

    private Vector3 velocitydir;
    private Vector2 input;
    private Vector3 velocity;
    private Vector3 tocursor;
    private float lastgroundangle;

    private Vector3 center;
    private GroundState grounded = GroundState.Grounded;
    private GroundState prevgrounded;
    private int currenttrash = 0;
    private Vector3 groundnormaldir = Vector3.up;
    private Vector3 facingdirection;
    private RaycastHit lastgroundhit;

    private bool paused = false;

    public Vector3 Center => center;
    public Vector3 FacingDir => velocitydir;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        prevgrounded = grounded;
        center = transform.position + collider.center;

        shootcooldown.Init();
        shootcooldown.time = 0;
        treadsmoketimer.Init();
        treadsmoketimer.frametime = 0;
    }

    void Start()
    {
        camera = GameObject.FindObjectOfType<PlayerCamera>();
        playerhud = GameObject.FindObjectOfType<PlayerHUD>();
        cursor = GameObject.FindObjectOfType<Cursor3D>();
        floatingui.SetTarget(this);

        treadsaudio.SetClip(sfxtreadsmove);
        treadsaudio.SetTargetVolume(treadsaudiomaxvolume);
    }

    void Update()
    {
        if (paused)
            return;

        input = new Vector2(GameInput.HorizontalInput(), GameInput.VerticalInput());
        if(input == Vector2.zero)
        {
            if(treadsaudio.State() != ESmartAudioState.Stopped && treadsaudio.State() != ESmartAudioState.Stopping)
                treadsaudio.BeginStop(20);
        }
        else
        {
            if(treadsaudio.State() != ESmartAudioState.Started && treadsaudio.State() != ESmartAudioState.Starting)
                treadsaudio.BeginStart(sfxtreadsmove, true, 10);
        }

        if (!shootcooldown.TimerReached())
            shootcooldown.Tick(Time.deltaTime);

        if(currentdialogue != null)
        {
            if(GameInput.Interact())
            {
                currentdialogue.BeginDialogue();

                if (treadsaudio.State() != ESmartAudioState.Stopped && treadsaudio.State() != ESmartAudioState.Stopping)
                    treadsaudio.BeginStop(20);

                paused = true;
            }
        }

        float treadsaudiolerp = Mathf.InverseLerp(0f, maxspeed, velocity.magnitude);
        float treadsaudiopitch = Mathf.Lerp(treadsaudiominpitch, treadsaudiomaxpitch, treadsaudiolerp);
        treadsaudio.SetPitch(treadsaudiopitch);

        // -- tf rotation
        Vector3 facingnoy = facingdirection;
        facingnoy.y = 0;

        Quaternion targetrot = Quaternion.LookRotation(facingnoy, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetrot, rotationsmoothing * Time.deltaTime);

        // -- head rotation
        Quaternion targetheadrot = Quaternion.identity;
        if (!cursor.HasIntersection)
        {
            Vector2 centerscreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 mousepos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            Vector2 screenpos = mousepos - centerscreen;

            float cursorangle = Mathf.Atan2(screenpos.y, screenpos.x);
            cursorangle *= Mathf.Rad2Deg;
            targetheadrot =  Quaternion.AngleAxis(cursorangle, Vector3.down) * Quaternion.Euler(headorientation);
        }
        else
        {
            tocursor = cursor.Position - head.position;
            tocursor.y = 0f;
            tocursor.Normalize();

            Vector3 cursorheadorient = new Vector3(headorientation.x, 0f, headorientation.z);
            targetheadrot =  Quaternion.LookRotation(tocursor) * Quaternion.Euler(cursorheadorient);
        }

        //Quaternion rotnoy = new Quaternion(transform.rotation.x, 0f, transform.rotation.z, transform.rotation.w);
        //targetheadrot = rotnoy * targetheadrot;
        head.rotation = Quaternion.Slerp(head.rotation, targetheadrot, headsmoothing * Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (paused)
            return;

        center = transform.position + collider.center;
        RaycastHit[] forwardhits = Physics.SphereCastAll(center, collider.radius, velocitydir, spherecastforwarddist, ground);
        RaycastHit forwardhitbest = new RaycastHit();
        forwardhitbest.distance = -1;
        foreach(RaycastHit hit in forwardhits)
        {
            float angle = Mathf.Abs(Vector3.Angle(hit.normal, -gravity.Direction));
            if (angle <= anglethreshold)
            {
                lastgroundangle = angle;
                forwardhitbest = hit;
            }
        }

        RaycastHit[] groundhits = Physics.SphereCastAll(center, collider.radius, gravity.Direction, spherecastdist, ground);
        if(groundhits.Length == 0 && forwardhitbest.distance == -1)
        {
            grounded = GroundState.InAir;
            rigidbody.velocity += gravity.Direction * gravity.Force;
        }
        else
        {
            int groundhitidx = 0;
            int i = 0;
            foreach(RaycastHit hit in groundhits)
            {
                if (hit.point != Vector3.zero)
                {
                    groundhitidx = i;
                    break;
                }

                ++i;
            }

            RaycastHit usehit = forwardhitbest.distance == -1 ? groundhits[groundhitidx] : forwardhitbest;

            groundnormaldir = usehit.normal;
            grounded = GroundState.Grounded;
            rigidbody.velocity = Vector3.zero;

            ProjectVelocity(usehit);
            if(!PositionOnGround(usehit))
            {
                Vector3 temp = Vector3.Cross(velocitydir, groundnormaldir);
                facingdirection = Vector3.Cross(temp, groundnormaldir);
            }

            lastgroundhit = usehit;
        }

        if (GameInput.Fire() && shootcooldown.TimerReached() && currenttrash > 0)
        {
            FireTrashBall();
            shootcooldown.Reset();
        }

        // -- we have changed grounded this frame
        if (grounded != prevgrounded)
        {
        }

        if (input == Vector2.zero)
        {
            velocity *= stopdamp;
        }
        else
        {
            Vector3 move = camera.TargetRightDirection() * input.x + camera.TargetForwardDirection() * input.y;
            velocity += move * acceleration;
            velocity = Vector3.ClampMagnitude(velocity, maxspeed);

            velocitydir = velocity.normalized;

            treadsmoketimer.Decrement();
            if(treadsmoketimer.TimerReached())
            {
                treadsmoketimer.Reset();

                GameObject treadparticles = GameObject.Instantiate(particlestreadsmoke, treadsmokeparticleemitter.position, treadsmokeparticleemitter.rotation);
                GameObject.Destroy(treadparticles, 2.0f);
            }
        }

        foreach(ScrollTexture t in treads)
        {
            t.SetScrollValue(0, Vector2.right * velocity.magnitude * treadspeed);
        }

        transform.position += velocity;

        prevgrounded = grounded;
    }

    private void ProjectVelocity(RaycastHit hit)
    {
        Vector3 wallbinormal = BinormalFromHitNormal(hit.normal);
        velocity = wallbinormal * velocity.magnitude;
    }

    private Vector3 BinormalFromHitNormal(Vector3 hitnormal)
    {
        Vector3 temp = Vector3.Cross(hitnormal, velocitydir);
        return Vector3.Cross(temp, hitnormal).normalized;
    }

    private void FireTrashBall()
    {
        Projectile trashball = GameObject.Instantiate(trashballprefab, firetransform.position, firetransform.rotation);
        Vector3 dir = cursor.HasIntersection ? tocursor : firetransform.forward;
        trashball.Initialize(dir, firespeed, velocity);

        currenttrash -= trashballcost;
        if (currenttrash < 0)
            currenttrash = 0;

        AudioManager.PlayRandomClip2D(sfxfire.sounds);
        UpdateUI();
    }

    public float CameraPredictionSimilarity()
    {
        return Vector3.Dot(velocity.normalized, Vector3.back) > 0.5f ? 10.0f : 1.0f;
    }

    public Vector3 CameraPrediction(float mult)
    {
        // -- harder to see in back
        return velocity * mult;
    }

    private bool PositionOnGround(RaycastHit hit)
    {
        if (hit.point == Vector3.zero)
        {
            if (Physics.Raycast(center, Vector3.down, out RaycastHit casehit, raycastdist, ground, QueryTriggerInteraction.Ignore))
            {
                hit = casehit;
            }
            else
                return false;
        }

        // -- pos
        transform.position = hit.point + hit.normal * collider.radius - collider.center;

        // -- rot
        Vector3 temp = Vector3.Cross(velocitydir, groundnormaldir);
        facingdirection = Vector3.Cross(temp, groundnormaldir);

        return true;
    }

    private void OnGUI()
    {
        return;

        Rect rect = new Rect(15f, 15f, 500f, 30f);
        float lineheight = 30f;

        GUI.Label(rect, string.Format("State: {0}", grounded.ToString())); rect.y += lineheight;
        GUI.Label(rect, string.Format("Last Ground Angle: {0}", lastgroundangle.ToString("0.00"))); rect.y += lineheight; 
        GUI.Label(rect, string.Format("Last Ground HitPos: {0}", lastgroundhit.point)); rect.y += lineheight;
        GUI.Label(rect, string.Format("Trash: {0}/{1}", currenttrash, maxtrash)); rect.y += lineheight;
        GUI.Label(rect, string.Format("Shoot Cooldown: {0}", shootcooldown.time)); rect.y += lineheight; 
    }

    public enum GroundState
    {
        Grounded = 0,
        InAir = 1
    }

    private void UpdateUI()
    {
        float fillratio = (float)currenttrash / (float)maxtrash;
        floatingui.SetFill(fillratio);
        playerhud.HUDSetFill(fillratio);
    }

    void OnTriggerEnter(Collider other)
    {
        if (paused)
            return;

        Collectable c = other.GetComponent<Collectable>();
        if(c != null)
        {
            if(c.type == CollectableType.Trash)
            {
                currenttrash += c.value;
                currenttrash = currenttrash > maxtrash ? currenttrash : currenttrash;

                UpdateUI();
            }
            c.Pickup();
        }

        Projectile p = other.GetComponent<Projectile>();
        if (p != null)
        {

        }

        DialogueInteraction i = other.GetComponent<DialogueInteraction>();
        if(i != null)
        {
            i.ActivateInteraction();
            currentdialogue = i;

            if(i.forceinteraction)
            {
                i.BeginDialogue();

                if (treadsaudio.State() != ESmartAudioState.Stopped && treadsaudio.State() != ESmartAudioState.Stopping)
                    treadsaudio.BeginStop(20);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        DialogueInteraction i = other.GetComponent<DialogueInteraction>();
        if (i != null)
        {
            i.DeactivateInteraction();
        }
    }

    public void SetPaused(bool paused)
    {
        this.paused = paused;
    }

    public void EndGame()
    {
        GameObject.Destroy(GameObject.Instantiate(particlesdeath, center, Quaternion.identity), 4.0f);
        camera.EndGame();
        gameObject.SetActive(false);
    }
}
