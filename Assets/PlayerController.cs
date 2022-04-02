using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 0.04f;
    public float maxspeed = 0.5f;
    public float stopdamp = 0.95f;

    [Header("Physics")]
    public new SphereCollider collider;
    public float spherecastdist = 1.0f;
    public GravityComponent gravity;
    public LayerMask ground;

    [Header("Rotations")]
    public Transform head;
    public Vector3 headorientation;
    public float headsmoothing = 6.0f;
    public float rotationsmoothing = 1.0f;

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

    private new PlayerCamera camera;
    private new Rigidbody rigidbody;
    private Cursor3D cursor;

    private Vector3 facingdirection;
    private Vector2 input;
    private Vector3 velocity;

    private Vector3 center;
    private GroundState grounded = GroundState.Grounded;
    private GroundState prevgrounded;
    private int currenttrash = 0;
    private Vector3 tocursor;

    public Vector3 Center => center;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        prevgrounded = grounded;
        center = transform.position + collider.center;
        shootcooldown.Init();
        shootcooldown.time = 0;
    }

    void Start()
    {
        camera = GameObject.FindObjectOfType<PlayerCamera>();
        cursor = GameObject.FindObjectOfType<Cursor3D>();
        floatingui.SetTarget(this);
    }

    void Update()
    {
        input = new Vector2(GameInput.HorizontalInput(), GameInput.VerticalInput());
        if(input == Vector2.zero)
        {

        }
        else
        {

        }

        shootcooldown.Tick(Time.deltaTime);

        tocursor = cursor.Position - head.position;
        tocursor.y = 0f;
        tocursor.Normalize();

        Quaternion targetheadrot = Quaternion.LookRotation(tocursor) * Quaternion.Euler(headorientation);

        head.rotation = Quaternion.Slerp(head.rotation, targetheadrot, headsmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(facingdirection, Vector3.up), rotationsmoothing * Time.deltaTime);
    }

    void FixedUpdate()
    {
        center = transform.position + collider.center;
        RaycastHit[] groundhits = Physics.SphereCastAll(center, collider.radius, gravity.Direction, spherecastdist, ground);
        if(groundhits.Length == 0)
        {
            grounded = GroundState.InAir;
            rigidbody.velocity += gravity.Direction * gravity.Force;
        }
        else
        {
            grounded = GroundState.Grounded;
            rigidbody.velocity = Vector3.zero;

            if (GameInput.Fire() && shootcooldown.TimerReached() && currenttrash > 0)
            {
                shootcooldown.Reset();

                Projectile trashball = GameObject.Instantiate(trashballprefab, firetransform.position, firetransform.rotation);
                trashball.Initialize(tocursor, firespeed, velocity);
                currenttrash -= trashballcost;

                if (currenttrash < 0)
                    currenttrash = 0;

                UpdateUI();
            }
        }

        // -- we have changed grounded this frame
        if(grounded != prevgrounded)
        {
            if(grounded == GroundState.Grounded)
            {
                PositionOnGround(groundhits[0]);
            }
            else
            {

            }
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

            facingdirection = velocity.normalized;
        }

        foreach(ScrollTexture t in treads)
        {
            t.SetScrollValue(0, Vector2.right * velocity.magnitude * treadspeed);
        }

        transform.position += velocity;

        prevgrounded = grounded;
    }

    public float CameraPredictionSimilarity()
    {
        return Vector3.Dot(velocity.normalized, Vector3.back) > 0.9f ? 10.0f : 1.0f;
    }

    public Vector3 CameraPrediction(float mult)
    {
        // -- harder to see in back
        return velocity * mult;
    }

    private void PositionOnGround(RaycastHit hit)
    {
        transform.position = hit.point + hit.normal * collider.radius - collider.center;
    }

    private void OnGUI()
    {
        Rect rect = new Rect(15f, 15f, 200f, 30f);
        float lineheight = 30f;

        GUI.Label(rect, string.Format("State: {0}", grounded.ToString()));
        rect.y += lineheight;
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
    }

    void OnTriggerEnter(Collider other)
    {
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
    }
}
