using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float cameraHeightOffset = 1f;

    [Header("Jump")]
    [SerializeField, Min(0f)] private float coyoteTime = 0.1f;
    [SerializeField, Min(0f)] private float jumpInputBuffer = 0.12f;
    [SerializeField, Range(0.1f, 1f)] private float jumpStrengthMultiplier = 0.8f;
    [SerializeField, Min(1f)] private float fallGravityMultiplier = 2.25f;

    [Header("Screen Shake")]
    [SerializeField, Min(0f)] private float screenShakeStrength = 0.035f;

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;

    private CharacterController controller;
    private Vector3 velocity;
    private Transform cameraTransform;
    private Vector3 cameraRestLocalPosition;
    private bool lockInputEnabled = true;
    private bool movementInputEnabled = true;
    private bool screenShakeEnabled;
    private float lastGroundedTime = float.NegativeInfinity;
    private float lastJumpPressedTime = float.NegativeInfinity;

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
            cameraRestLocalPosition = cameraTransform.localPosition;
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

        Vector2 input = movementInputEnabled && moveAction != null
            ? moveAction.action.ReadValue<Vector2>()
            : Vector2.zero;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        bool isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;

            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }

        if (movementInputEnabled &&
            jumpAction != null &&
            jumpAction.action.WasPressedThisFrame())
        {
            lastJumpPressedTime = Time.time;
        }

        bool jumpWasPressedRecently =
            Time.time - lastJumpPressedTime <= jumpInputBuffer;
        bool wasGroundedRecently =
            Time.time - lastGroundedTime <= coyoteTime;

        if (movementInputEnabled && jumpWasPressedRecently && wasGroundedRecently)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity) *
                jumpStrengthMultiplier;
            lastJumpPressedTime = float.NegativeInfinity;
            lastGroundedTime = float.NegativeInfinity;
        }

        float gravityMultiplier = velocity.y < 0f
            ? fallGravityMultiplier
            : 1f;
        velocity.y += gravity * gravityMultiplier * Time.deltaTime;

        CollisionFlags collisionFlags = controller.Move(
            (move * moveSpeed + Vector3.up * velocity.y) * Time.deltaTime
        );

        if ((collisionFlags & CollisionFlags.Below) != 0)
        {
            lastGroundedTime = Time.time;

            if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }

        if (cameraTransform == null)
        {
            return;
        }

        if (lockInputEnabled && lookAction != null)
        {
            Vector2 look = lookAction.action.ReadValue<Vector2>() * mouseSensitivity;
            transform.Rotate(Vector3.up * look.x);
            cameraTransform.Rotate(Vector3.left * look.y);
        }

        cameraTransform.localPosition = screenShakeEnabled
            ? cameraRestLocalPosition + Random.insideUnitSphere * screenShakeStrength
            : cameraRestLocalPosition;
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

    public void SetMovementInputEnabled(bool isEnabled)
    {
        movementInputEnabled = isEnabled;
    }

    public void SetScreenShake(bool isEnabled)
    {
        screenShakeEnabled = isEnabled;

        if (!isEnabled && cameraTransform != null)
        {
            cameraTransform.localPosition = cameraRestLocalPosition;
        }
    }
}
