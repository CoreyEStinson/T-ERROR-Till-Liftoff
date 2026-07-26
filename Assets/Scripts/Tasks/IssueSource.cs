using System;
using UnityEngine;

public enum RocketSystem
{
    Control,
    Engine,
    Electrical,
    Fuel
}

public class IssueSource : MonoBehaviour
{
    [Header("Issue Details")] 
    [SerializeField] private string alertText = "Pressure anomaly";
    [SerializeField] private string roomName = "Fuel";
    [SerializeField] private RocketSystem rocketSystem = RocketSystem.Fuel;
    [SerializeField] private bool isCritical = true;
    [SerializeField, Range(0f, 100f)] private float healthPenalty = 20f;
    [SerializeField, Min(1)] private int spawnWeight = 1;

    [Header("References")]
    [SerializeField] private RepairTask repairTask;
    [SerializeField] private GameObject[] failureVisuals;

    public string AlertText => alertText;
    public string RoomName => roomName;
    public RocketSystem System => rocketSystem;
    public bool IsCritical => isCritical;
    public float HealthPenalty => isCritical ? healthPenalty : 0f;
    public float CurrentHealthPenalty =>
        IsActive && isCritical ? currentHealthPenalty : 0f;
    public float LastResolvedHealthPenalty => lastResolvedHealthPenalty;
    public int SpawnWeight => spawnWeight;
    public bool IsActive { get; private set; }
    public bool IsAvailableForSelection =>
        !IsActive && Time.time >= nextAvailableTime;

    public event Action<IssueSource> IssueActivated;
    public event Action<IssueSource> IssueResolved;
    public event Action<IssueSource> IssueFailed;

    private float currentHealthPenalty;
    private float lastResolvedHealthPenalty;
    private float nextAvailableTime;

    private void Awake()
    {
        if (repairTask == null)
        {
            repairTask = GetComponent<RepairTask>();
        }
    }

    private void OnEnable()
    {
        if (repairTask == null)
        {
            return;
        }

        repairTask.TaskCompleted += HandleTaskCompleted;
        repairTask.TaskFailed += HandleTaskFailed;
    }

    private void OnDisable()
    {
        if (repairTask == null)
        {
            return;
        }

        repairTask.TaskCompleted -= HandleTaskCompleted;
        repairTask.TaskFailed -= HandleTaskFailed;
    }

    public void ActivateIssue()
    {
        if (IsActive || repairTask == null)
        {
            return;
        }

        IsActive = true;
        currentHealthPenalty = 0f;
        lastResolvedHealthPenalty = 0f;
        repairTask.ActivateTask();
        SetFailureVisuals(true);

        IssueActivated?.Invoke(this);
    }

    public void DeactivateIssue()
    {
        IsActive = false;
        currentHealthPenalty = 0f;

        if (repairTask != null)
        {
            repairTask.DeactivateTask();
        }

        SetFailureVisuals(false);
    }

    public bool ApplyHealthPenaltyTick()
    {
        if (!IsActive || !isCritical || currentHealthPenalty >= healthPenalty)
        {
            return false;
        }

        currentHealthPenalty = Mathf.Min(
            currentHealthPenalty + 1f,
            healthPenalty
        );

        return true;
    }

    public void StartSelectionCooldown(float delay)
    {
        nextAvailableTime = Time.time + Mathf.Max(0f, delay);
    }

    private void HandleTaskCompleted(RepairTask completedTask)
    {
        if (!IsActive)
        {
            return;
        }

        lastResolvedHealthPenalty = currentHealthPenalty;
        DeactivateIssue();
        IssueResolved?.Invoke(this);
    }

    private void HandleTaskFailed(RepairTask failedTask)
    {
        if (IsActive)
        {
            IssueFailed?.Invoke(this);
        }
    }

    private void SetFailureVisuals(bool visible)
    {
        foreach (GameObject visual in failureVisuals)
        {
            if (visual != null)
            {
                visual.SetActive(visible);
            }
        }
    }

}
