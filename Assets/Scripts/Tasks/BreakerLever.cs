using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

public class BreakerLever : Interactable
{
    [Header("BreakerInformation")]
    [SerializeField] private string breakerName = "Red";

    [Header("Visual References")]
    [SerializeField] private Transform leverHandle;
    [SerializeField] private GameObject sequenceIndicator;

    [Header("Lever Rotation")]
    [SerializeField] private Vector3 localRotationAxis = Vector3.right;
    [SerializeField] private float offAngle = -25f;
    [SerializeField] private float onAngle = 25f;
    [SerializeField] private float rotationDuration = 0.12f;

    private Coroutine rotationRoutine;
    private BreakerTask breakerTask;
    private Quaternion startingLocalRotation;

    private void Awake()
    {
        breakerTask = GetComponentInParent<BreakerTask>();

        if (leverHandle == null)
        {
            leverHandle = transform;
        } 

        startingLocalRotation = leverHandle.localRotation;
        leverHandle.localRotation = GetTargetRotation(false);
        SetSequenceIndicator(false);
    }

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        if (breakerTask == null || !breakerTask.CanUseLever(this, equippedTool))
        {
            return string.Empty;
        }

        return $"[E] Flip {breakerName} breaker";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return breakerTask != null && breakerTask.CanUseLever(this, equippedTool);
    }

    public override void Interact(ToolType equippedTool)
    {
        breakerTask?.TryFlipLever(this, equippedTool);
    }

    public void SetFlipped(bool isOn)
    {
        Quaternion targetRotation = GetTargetRotation(isOn);

        if (rotationRoutine != null)
        {
            StopCoroutine(rotationRoutine);
        }

        if (rotationDuration <= 0f)
        {
            leverHandle.localRotation = targetRotation;
            return;
        }

        rotationRoutine = StartCoroutine(RotateLever(targetRotation));
    }

    private Quaternion GetTargetRotation(bool isOn)
    {
        float angle = isOn ? onAngle : offAngle;

        return startingLocalRotation * 
            Quaternion.AngleAxis(angle, localRotationAxis);
    }

    private IEnumerator RotateLever(Quaternion targetRotation)
    {
        Quaternion startRotation = leverHandle.localRotation;
        float elapsed = 0f;

        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / rotationDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            leverHandle.localRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                easedProgress
            );

            yield return null;
        }

        leverHandle.localRotation = targetRotation;
        rotationRoutine = null;
    }

    public void SetSequenceIndicator(bool isVisible)
    {
        if (sequenceIndicator != null)
        {
            sequenceIndicator.SetActive(isVisible);
        }
    }
}