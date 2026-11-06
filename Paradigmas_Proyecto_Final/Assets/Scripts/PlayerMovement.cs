

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Camara")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.12f;

    private CharacterController controller;
    private float verticalVelocity;
    private float cameraPitch;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        LockCursor();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        MovePlayer();
        LookAround();
    }

    private void MovePlayer()
    {
        Vector3 direction = Vector3.zero;
        Keyboard keyboard = Keyboard.current;

        if (keyboard.wKey.isPressed)
            direction += transform.forward;

        if (keyboard.sKey.isPressed)
            direction -= transform.forward;

        if (keyboard.aKey.isPressed)
            direction -= transform.right;

        if (keyboard.dKey.isPressed)
            direction += transform.right;

        direction = direction.normalized;

        // Comprobar si el jugador esta en el suelo
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // Saltar solamente si esta tocando el suelo
        if (controller.isGrounded &&
            keyboard.spaceKey.wasPressedThisFrame)
        {
            verticalVelocity = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        // Aplicar gravedad
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * speed +
                           Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void LookAround()
    {
        if (Mouse.current == null ||
            Cursor.lockState != CursorLockMode.Locked ||
            playerCamera == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float mouseX = mouseDelta.x * mouseSensitivity;
        float mouseY = mouseDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

        playerCamera.localRotation =
            Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
