using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

public class ValveTask : RepairTask
{
    [Header("Valve References")]
    [SerializeField] private Transform valveWheel;
    [SerializeField] private FirstPersonController playerController;

    [Header("Rotation Settings")]
    [SerializeField] private Vector3 localRotationAxis = Vector3.forward;
    [SerializeField] private float degreesRequired = 360;
    [SerializeField] private float degreesPerMousePixel = 1.5f;
    [SerializeField] private int correctTurnDirection = 1;

    [Header("Failure")]
    [SerializeField] private bool resetProgressWhenReleased = true;
    [SerializeField] private float resetDuration = 0.35f;
    [SerializeField] private UnityEvent onValveFailed;

    private Quaternion startingLocalRotation;
    private float currentDegrees;
    private bool isTurning;
    private Coroutine resetRoutine;

    private void Awake()
    {
        if (valveWheel == null)
        {
            valveWheel = transform;
        }

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }

        startingLocalRotation = valveWheel.localRotation;

        correctTurnDirection = correctTurnDirection >= 0 ? 1 : -1;
    }

    private void OnDisable()
    {
        StopTurning();       
    }

    protected override string GetActivePrompt()
    {
        if (isTurning)
        {
            float percent = currentDegrees / degreesRequired * 100f;
            return $"Hold LMB: Turn Valve ({percent:0})%";
        }

        return "Hold LMB: Turn Valve";
    }

    protected override void HandleFocusedInput()
    {
        if (Mouse.current == null)
        {
            return;
        }

        if (!isTurning)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                BeginTurning();
            }

            return;
        }

        if (!Mouse.current.leftButton.isPressed)
        {
            HandleReleasedEarly();
            return;
        }

        float mouseX = Mouse.current.delta.ReadValue().x;

        // Mouse motion in the correct direction adds progress.
        // Mouse motion in the wrong direction removes progress.
        float signedRotation = mouseX * degreesPerMousePixel * correctTurnDirection;

        currentDegrees = Mathf.Clamp(
            currentDegrees + signedRotation,
            0f,
            degreesRequired
        );

        UpdateWheelVisual();

        if (currentDegrees >= degreesRequired)
        {
            StopTurning();
            CompleteTask();
        }
    }

    protected override void HandleFocusLost()
    {
        if (isTurning)
        {
            HandleReleasedEarly();
        }
    }

    private void BeginTurning()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        isTurning = true;
        playerController?.SetLookInputEnabled(false);
    }

    private void HandleReleasedEarly()
    {
        StopTurning();

        if (!resetProgressWhenReleased || currentDegrees <= 0f)
        {
            return;
        }

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
        }

        resetRoutine = StartCoroutine(ResetValveRoutine(currentDegrees));

        FailTask();
        onValveFailed?.Invoke();
    }

    private IEnumerator ResetValveRoutine(float startingDegrees)
    {
        float elapsed = 0f;

        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / resetDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            currentDegrees = Mathf.Lerp(startingDegrees, 0f, easedProgress);
            UpdateWheelVisual();

            yield return null;
        }

        currentDegrees = 0f;
        UpdateWheelVisual();

        resetRoutine = null;
    }

    private void StopTurning()
    {
        isTurning = false;
        playerController?.SetLookInputEnabled(true);
    }

    private void UpdateWheelVisual()
    {
        valveWheel.localRotation = 
            startingLocalRotation * 
            Quaternion.AngleAxis(
                currentDegrees * correctTurnDirection,
                localRotationAxis
            );
    }

    protected override void OnTaskActivated()
    {
        StopTurning();

        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        currentDegrees = 0f;
        UpdateWheelVisual();
    }

    protected override void OnTaskDeactivated()
    {
        StopTurning();
    }
}
