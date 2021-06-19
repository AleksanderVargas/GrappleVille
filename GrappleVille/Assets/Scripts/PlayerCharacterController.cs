using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    //Public
    public Transform Camera;
    public float mouseSensitivity = 500f;
    public float movementSpeed = 10f;
    public float jumpForce = 10f;
    public float gravity = -9.18f;

    //Private
    private CharacterController controller;
    private float mouseX;
    private float mouseY;
    private float viewY = 0f;
    private float distGround;
    public Vector3 velocity;



    // Start is called before the first frame update
    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        distGround = controller.bounds.extents.y;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distGround + 0.1f);
    }

    // Update is called once per frame
    void Update()
    {
        //Frame Based input
        mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        //VIEW
        //Rotate Player across X Axis when looking
        gameObject.GetComponent<Transform>().Rotate(Vector3.up * mouseX);
        //Rotate only Camera when looking vertically
        viewY -= mouseY;
        viewY = Mathf.Clamp(viewY, -90f, 90f);
        Camera.localRotation = Quaternion.Euler(viewY, 0f, 0f);

        //MOVEMENT
        //X|Z Movement
        Vector3 movement = transform.right * inputX + transform.forward * inputZ;
        controller.Move(movement * movementSpeed * Time.deltaTime);
        //Gravity
        if (IsGrounded() && velocity.y < -2.0f)
        {
            velocity.y = -2f;
        }
        if (!IsGrounded())
        {
            velocity.y += (gravity * Time.deltaTime);
        }
        //Jumping
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            velocity.y = jumpForce;
        }
        controller.Move(velocity * Time.deltaTime);


    }
}
