using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float cameraHeightOffset = 1f;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    private CharacterController controller;
    private Vector3 velocity;
    private Transform cameraTransform;
    private bool lockInputEnabled = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
            cameraTransform.position = new Vector3(
                transform.position.x,
                transform.position.y + cameraHeightOffset,
                transform.position.z);
            cameraTransform.parent = transform;
        }
    }

    private void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
        LockCursor();
    }

    private void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        jumpAction?.action.Disable();
        UnlockCursor();
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked &&
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            LockCursor();
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            UnlockCursor();
        }

        Vector2 input = moveAction != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity.
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (controller.isGrounded && jumpAction != null && jumpAction.action.WasPressedThisFrame())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (cameraTransform == null || lookAction == null)
        {
            return;
        }

        if (!lockInputEnabled)
        {
            return;
        }

        Vector2 look = lookAction.action.ReadValue<Vector2>() * mouseSensitivity;
        transform.Rotate(Vector3.up * look.x);
        cameraTransform.Rotate(Vector3.left * look.y);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            LockCursor();
        }
        else
        {
            UnlockCursor();
        }
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetLookInputEnabled(bool isEnabled)
    {
        lockInputEnabled = isEnabled;
    }
}
