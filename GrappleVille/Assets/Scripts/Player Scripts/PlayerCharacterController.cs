using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private State state;
    [SerializeField] private GameObject debugHitPoint;
    [SerializeField] private const float NORMAL_FOV = 60f;
    [SerializeField] private const float GRAPPLE_FOV = 100f;

    //Public
    public float mouseSensitivity = 500f;
    public float movementSpeed = 10f;
    public float grappleShootingSpeed = 140f;
    public float grappleSpeedMultiplier = 2f;

    public float minGrappleSpeed = 10;
    public float maxGrappleSpeed = 50f;
    public float grappleTime = 3f;
    public float minGrappleDist = 1.5f;
    public float jumpForce = 10f;
    public float gravity = -9.18f;


    //Private
    private CharacterController controller;
    private ParticleSystem zoomParticleSystem;
    private GameObject particleSystemReference;
    private GameObject grapple;
    private GameObject Camera;
    private CameraFOV cameraFOV;
    private Transform debugHitPointTransform;
    private float grappleSize = 0f;
    private float mouseX;
    private float mouseY;
    private float cameraVerticalAngle = 0f;
    private float distGround;
    private float grappleTimer;
    private bool aiming = false;
    private Vector3 velocity;
    private Vector3 grapplePosition;


    private enum State
    {
        Normal,
        ShootingGrapple,
        Grappling
    }


    private void Awake()
    {
        //Initialize Variables
        controller = GetComponent<CharacterController>();
        Camera = transform.Find("Main Camera").gameObject;
        cameraFOV = Camera.GetComponent<CameraFOV>();
        grapple = transform.Find("Grapple").gameObject;
        particleSystemReference = transform.Find("ParticleSystemReference").gameObject;
        zoomParticleSystem = particleSystemReference.transform.Find("Zooming Particles").GetComponent<ParticleSystem>();

        //Initialize Scene
        SetCameraFOV(NORMAL_FOV);
        debugHitPointTransform = debugHitPoint.transform;
        SetGrappleActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Start is called before the first frame update
    void Start()
    {
        distGround = controller.bounds.extents.y;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distGround + 0.1f);
    }

    // XX--FUNCTIONS --XX
    private void SetCameraFOV(float fov)
    {
        cameraFOV.SetCameraFOV(fov);
    }

    private void SetAiming(bool b)
    {
        aiming = b;
        debugHitPoint.GetComponent<MeshRenderer>().enabled = b;
    }
    private void SetGrappleActive(bool b)
    {
        grapple.SetActive(b);
    }
    private void StartGrappling()
    {
        state = State.Grappling;
        zoomParticleSystem.Play();
    }
    private void StopGrappling()
    {
        zoomParticleSystem.Stop();
        SetGrappleActive(false);
        SetCameraFOV(NORMAL_FOV);
        state = State.Normal;
    }

    private void HandleJump()
    {
        //Jumping
        //Check Actions
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            velocity.y = jumpForce;
        }
    }

    private void HandleAiming()
    {
        //check AIMING
        if (Input.GetButtonDown("Grapple"))
        {
            SetAiming(true);
        }
        else if (Input.GetButtonUp("Grapple"))
        {
            SetAiming(false);
            if (state == State.Normal)
            {
                //Grapple
                if (Vector3.Distance(debugHitPointTransform.position, transform.position) > minGrappleDist)
                {
                    //Shoot Grapple To destination
                    grapplePosition = debugHitPointTransform.position;
                    grappleTimer = grappleTime;
                    grappleSize = 0f;
                    grapple.transform.localScale = Vector3.zero;
                    SetGrappleActive(true);
                    state = State.ShootingGrapple;
                }
            }
        }
        if (aiming)
        {
            //Check if There is a collidable instance in front of the players aim
            if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out RaycastHit raycastHit))
            {
                debugHitPointTransform.position = raycastHit.point;
            }
            else
            {
                debugHitPointTransform.position = transform.position;
            }
        }
        else
        {
            debugHitPointTransform.position = transform.position;
        }
    }

    private void HandleShootingGrapple()
    {
        grapple.transform.LookAt(grapplePosition);
        particleSystemReference.transform.LookAt(grapplePosition);
        grappleSize += grappleShootingSpeed * Time.deltaTime;
        grapple.transform.localScale = new Vector3(1, 1, grappleSize);

        if (grappleSize >= Vector3.Distance(transform.position, grapplePosition))
        {
            //START GRAPPLING
            StartGrappling();
        }

    }

    private void HandleCameraView()
    {
        //VIEW
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //Rotate Player across X Axis when looking
        gameObject.GetComponent<Transform>().Rotate(Vector3.up * mouseX);
        //Rotate only Camera when looking vertically
        cameraVerticalAngle -= mouseY;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -90f, 90f);
        Camera.transform.localRotation = Quaternion.Euler(cameraVerticalAngle, 0f, 0f);
    }

    private void HandleMovement()
    {
        //MOVEMENT
        //X|Z Movement
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        Vector3 movement = transform.right * inputX + transform.forward * inputZ;
        controller.Move(movement * movementSpeed * Time.deltaTime);

    }

    private void HandleGrappleShotMovement()
    {
        //Set Camera
        SetCameraFOV(GRAPPLE_FOV);

        //Get distance
        float grappleDist = Vector3.Distance(transform.position, grapplePosition);

        //Assert Grapple positioning
        grapple.transform.LookAt(grapplePosition);
        grapple.transform.localScale = new Vector3(1, 1, grappleDist);
        //Assert Zooming Particle affect is facing proper direction
        particleSystemReference.transform.LookAt(grapplePosition);
        //Get the normalized ("1'ed" vector) direction
        Vector3 grappleDir = (grapplePosition - transform.position).normalized;
        float grappleSpeed = Mathf.Clamp(grappleDist, minGrappleSpeed, maxGrappleSpeed);
        grappleSpeed *= grappleSpeedMultiplier;

        //Check Jump-Cancel
        if (Input.GetButtonDown("Jump"))
        {
            velocity = grappleDir * minGrappleSpeed * Time.deltaTime;
            velocity.y += jumpForce;
            StopGrappling();
            return;
        }

        //Move Character in direction
        controller.Move(grappleDir * grappleSpeed * Time.deltaTime);

        //Check if weve reached the destination
        if (Vector3.Distance(grapplePosition, transform.position) < minGrappleDist || grappleTimer <= 0)
        {
            StopGrappling();
        }
        else //Still grappling
        {
            //Assure no growing changes in gravity
            velocity.y = 0;
            //Tick GrappleTimer
            grappleTimer -= Time.deltaTime;
        }
    }

    private void HandleGravity()
    {
        //Gravity
        //If on ground, reset gravity velocity.
        if (IsGrounded() && velocity.y < -2.0f)
        {
            velocity.y = -2f;
        }
        else if (!IsGrounded())
        {
            //If Not grounded, then apply slowly increasing gravity
            velocity.y += (gravity * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Frame Based input
        switch (state)
        {
            default:
            case State.Normal:
                HandleCameraView();
                HandleMovement();
                HandleGravity();
                HandleJump();
                HandleAiming();
                break;
            case State.ShootingGrapple:
                HandleShootingGrapple();
                HandleCameraView();
                HandleMovement();
                HandleGravity();
                HandleJump();
                HandleAiming();
                break;
            case State.Grappling:
                HandleCameraView();
                HandleAiming();
                HandleGrappleShotMovement();
                break;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
