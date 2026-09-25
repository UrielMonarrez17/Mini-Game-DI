
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 120f;
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Camera")]
    public Transform playerCamera;
    public float cameraDistance = 4f;
    public float cameraHeight = 2f;
    public float cameraAngle = 15f;

    private CharacterController controller;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UpdateCamera();
    }

    void Update()
    {
        MovePlayer();
        UpdateCamera();
    }

    void MovePlayer()
    {
        float move = 0f;

        if (Input.GetKey(KeyCode.W))
            move = -1f;

        if (Input.GetKey(KeyCode.S))
            move = 1f;

        float rotation = 0f;

        if (Input.GetKey(KeyCode.A))
            rotation = -1f;

        if (Input.GetKey(KeyCode.D))
            rotation = 1f;

        transform.Rotate(0f,rotation * rotationSpeed * Time.deltaTime,0f);

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 movement = transform.forward * moveSpeed * move;

        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    void UpdateCamera()
    {
        if (playerCamera == null)
            return;

        playerCamera.localPosition = new Vector3(0f,cameraHeight,-cameraDistance);

        playerCamera.localRotation = Quaternion.Euler(cameraAngle,0f,0f);
    }
}