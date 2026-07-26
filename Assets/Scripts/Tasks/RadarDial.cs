using System.Collections;
using UnityEngine;

public class RadarDial : Interactable
{
    [Header("Visuals")]
    [SerializeField] private Transform dialVisual;
    [SerializeField] private Renderer statusRenderer;
    [SerializeField] private Vector3 localRotationAxis = Vector3.forward;
    [SerializeField] private float degreesPerStep = 45f;
    [SerializeField, Min(0f)] private float rotationDuration = 0.15f;

    private RadarTask radarTask;
    private Quaternion startingLocalRotation;
    private int currentStep;
    private int targetStep;
    private bool isConfigured;
    private Coroutine rotationRoutine;

    public int CurrentStep => currentStep;
    public int TargetStep => targetStep;
    public bool IsAligned => isConfigured && currentStep == targetStep;

    private void Awake()
    {
        radarTask = GetComponentInParent<RadarTask>();

        if (dialVisual == null)
        {
            dialVisual = transform;
        }

        startingLocalRotation = dialVisual.localRotation;
    }

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        if (radarTask == null || !radarTask.CanAdjustDial(this, equippedTool))
        {
            return string.Empty;
        }

        return "[E] Adjust radar dial";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return radarTask != null && radarTask.CanAdjustDial(this, equippedTool);
    }

    public override void Interact(ToolType equippedTool)
    {
        radarTask?.AdjustDial(this, equippedTool);
    }

    public void Configure(int startingStep, int newTargetStep, int totalSteps)
    {
        currentStep = Mathf.Clamp(startingStep, 0, totalSteps - 1);
        targetStep = Mathf.Clamp(newTargetStep, 0, totalSteps - 1);
        isConfigured = true;

        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
            rotationRoutine = null;
        }

        SetRotationImmediately();
        UpdateStatusColor();
    }

    public void Advance(int totalSteps)
    {
        currentStep = (currentStep + 1) % totalSteps;

        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
        }

        rotationRoutine = StartCoroutine(RotateToCurrentStep());
        UpdateStatusColor();
    }

    private IEnumerator RotateToCurrentStep()
    {
        if (dialVisual == null || rotationDuration <= 0f)
        {
            SetRotationImmediately();
            rotationRoutine = null;
            yield break;
        }

        Quaternion startRotation = dialVisual.localRotation;
        Quaternion targetRotation = GetCurrentStepRotation();
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / rotationDuration);
            dialVisual.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                Mathf.SmoothStep(0f, 1f, progress)
            );

            yield return null;
        }

        dialVisual.localRotation = targetRotation;
        rotationRoutine = null;
    }

    private void SetRotationImmediately()
    {
        if (dialVisual != null)
        {
            dialVisual.localRotation = GetCurrentStepRotation();
        }
    }

    private Quaternion GetCurrentStepRotation()
    {
        return startingLocalRotation * Quaternion.AngleAxis(
            currentStep * degreesPerStep,
            localRotationAxis
        );
    }

    private void UpdateStatusColor()
    {
        if (statusRenderer != null)
        {
            statusRenderer.material.color = IsAligned ? Color.green : Color.red;
        }
    }
}
