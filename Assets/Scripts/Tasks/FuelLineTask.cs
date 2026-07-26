using System.Collections;
using TMPro;
using UnityEngine;

public class FuelLineTask : RepairTask
{
    [Header("Gauge")]
    [SerializeField] private Transform gaugeNeedle;
    [SerializeField] private TMP_Text gaugeReadout;
    [SerializeField] private Renderer[] purgeButtonRenderers;
    [SerializeField] private Vector3 localRotationAxis = Vector3.forward;
    [SerializeField] private float minimumNeedleAngle = -90f;
    [SerializeField] private float maximumNeedleAngle = 90f;
    [SerializeField, Min(0.1f)] private float pressureCycleDuration = 2.5f;
    [SerializeField, Min(0f)] private float resetDuration = 0.35f;

    [Header("Safe Pressure")]
    [SerializeField, Range(0f, 1f)] private float minimumSafePressure = 0.3f;
    [SerializeField, Range(0f, 1f)] private float maximumSafePressure = 0.7f;
    [SerializeField, Range(0.01f, 0.5f)] private float safePressureTolerance = 0.08f;

    [Header("Purge Button Colors")]
    [SerializeField] private Color unsafeButtonColor = Color.red;
    [SerializeField] private Color safeButtonColor = Color.green;

    private Quaternion startingLocalRotation;
    private float pressure;
    private float safePressure;
    private float elapsedTime;
    private Coroutine resetRoutine;

    public override bool UsesUniversalInteraction => true;

    private void Awake()
    {
        if (gaugeNeedle == null)
        {
            gaugeNeedle = transform;
        }

        startingLocalRotation = gaugeNeedle.localRotation;
    }

    private void Update()
    {
        if (!IsTaskActive || IsCompleted || resetRoutine != null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
        pressure = Mathf.PingPong(elapsedTime / pressureCycleDuration, 1f);
        UpdateGaugeVisual();
    }

    protected override string GetActivePrompt()
    {
        return $"[E] Purge fuel at {safePressure * 100f:0}%";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return IsTaskActive &&
            !IsCompleted &&
            resetRoutine == null &&
            HasRequiredTool(equippedTool);
    }

    public override void Interact(ToolType equippedTool)
    {
        if (!CanInteract(equippedTool))
        {
            return;
        }

        if (Mathf.Abs(pressure - safePressure) <= safePressureTolerance)
        {
            CompleteTask();
        }
        else
        {
            FailTask();
            ResetPressure();
        }
    }

    protected override void HandleFocusedInput() { }

    protected override void OnTaskActivated()
    {
        float maximumSafePressure = Mathf.Max(
            minimumSafePressure,
            this.maximumSafePressure
        );
        safePressure = Random.Range(minimumSafePressure, maximumSafePressure);
        UpdateGaugeVisual();
    }

    protected override void OnTaskDeactivated()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
            resetRoutine = null;
        }

        SetPurgeButtonColor(safeButtonColor);
    }

    private void ResetPressure()
    {
        if (resetRoutine != null)
        {
            StopCoroutine(resetRoutine);
        }

        resetRoutine = StartCoroutine(ResetPressureRoutine(pressure));
    }

    private IEnumerator ResetPressureRoutine(float startingPressure)
    {
        if (resetDuration <= 0f)
        {
            pressure = 0f;
            elapsedTime = 0f;
            UpdateGaugeVisual();
            resetRoutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < resetDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / resetDuration);
            pressure = Mathf.Lerp(startingPressure, 0f, Mathf.SmoothStep(0f, 1f, progress));
            UpdateGaugeVisual();

            yield return null;
        }

        elapsedTime = 0f;
        pressure = 0f;
        UpdateGaugeVisual();
        resetRoutine = null;
    }

    private void UpdateGaugeVisual()
    {
        if (gaugeNeedle != null)
        {
            float angle = Mathf.Lerp(
                minimumNeedleAngle,
                maximumNeedleAngle,
                pressure
            );
            gaugeNeedle.localRotation = startingLocalRotation *
                Quaternion.AngleAxis(angle, localRotationAxis);
        }

        if (gaugeReadout != null)
        {
            gaugeReadout.text =
                $"PRESSURE: {pressure * 100f:0}%\nSAFE RANGE: {safePressure * 100f:0}%";
        }

        bool isSafePressure = !IsTaskActive ||
            Mathf.Abs(pressure - safePressure) <= safePressureTolerance;
        SetPurgeButtonColor(
            isSafePressure ? safeButtonColor : unsafeButtonColor
        );
    }

    private void SetPurgeButtonColor(Color color)
    {
        if (purgeButtonRenderers == null)
        {
            return;
        }

        foreach (Renderer buttonRenderer in purgeButtonRenderers)
        {
            if (buttonRenderer == null)
            {
                continue;
            }

            foreach (Material material in buttonRenderer.materials)
            {
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", color);
                }
            }
        }
    }
}
