using System.Collections.Generic;
using UnityEngine;

public class IssueManager : MonoBehaviour
{
    [Header("Issue Sources")] 
    [SerializeField] private IssueSource[] issueSources;

    [Header("Spawning")]
    [SerializeField, Min(1)] private int maxActiveIssues = 3;
    [SerializeField] private float minimumSpawnDelay = 25f;
    [SerializeField] private float maxSpawnDelay = 40f;
    [Tooltip("How long a repaired task must wait before it can become an issue again.")]
    [SerializeField, Min(0f)] private float completedTaskCooldown = 15f;
    [SerializeField, Range(0f, 1f)] private float failureInconvenienceChance = 0.15f;

    [Header("Board")]
    [SerializeField] private ControlBoard controlBoard;

    [Header("Launch Requirements")]
    [SerializeField, Range(0f, 100f)] private float requiredLaunchHealth = 90f;

    [Header("Health Loss")]
    [Tooltip("Each active critical issue loses 1% health per tick until it reaches its Health Penalty.")]
    [SerializeField, Min(0.01f)] private float minimumHealthTickDelay = 1f;
    [SerializeField, Min(0.01f)] private float maximumHealthTickDelay = 2f;

    [Header("Final 30 Seconds")]
    [SerializeField, Min(0.01f)] private float finalThirtyMinimumHealthTickDelay = 0.3f;
    [SerializeField, Min(0.01f)] private float finalThirtyMaximumHealthTickDelay = 0.7f;

    [Header("Health Recovery")]
    [Tooltip("Health recovers by 1% per tick after an issue is repaired.")]
    [SerializeField, Min(0.01f)] private float minimumHealthRecoveryTickDelay = 0.75f;
    [SerializeField, Min(0.01f)] private float maximumHealthRecoveryTickDelay = 1.25f;

    [Header("References")]
    [SerializeField] private LaunchManager launchManager;

    private readonly List<IssueSource> activeIssues = new();
    private readonly float[] recoveringHealthPenalty = new float[4];
    private float nextSpawnTime;
    private float nextHealthTickTime;
    private float nextHealthRecoveryTickTime;
    private bool spawningEnabled = true;

    public IReadOnlyList<IssueSource> ActiveIssues => activeIssues;
    public float RequiredLaunchHealth => requiredLaunchHealth;
    public int RoundedRequiredLaunchHealth =>
        Mathf.RoundToInt(requiredLaunchHealth);

    private void Awake()
    {
        if (launchManager == null)
        {
            launchManager = FindAnyObjectByType<LaunchManager>();
        }

        if (issueSources == null || issueSources.Length == 0)
        {
            issueSources = FindObjectsByType<IssueSource>();
        }

        foreach (IssueSource source in issueSources)
        {
            if (source == null)
            {
                continue;
            }

            source.IssueActivated += HandleIssueActivated;
            source.IssueResolved += HandleIssueResolved;
            source.IssueFailed += HandleIssueFailed;
        }
    }

    private void Start()
    {
        foreach (IssueSource source in issueSources)
        {
            source?.DeactivateIssue();
        }

        ScheduleNextIssue(true);
        ScheduleNextHealthTick();
        ScheduleNextHealthRecoveryTick();
        RefreshBoard();
    }

    private void Update()
    {
        if (Time.time >= nextHealthTickTime)
        {
            ApplyHealthPenaltyTick();
            ScheduleNextHealthTick();
        }

        if (Time.time >= nextHealthRecoveryTickTime)
        {
            ApplyHealthRecoveryTick();
            ScheduleNextHealthRecoveryTick();
        }

        if (!spawningEnabled || 
            Time.time < nextSpawnTime ||
            activeIssues.Count >= maxActiveIssues)
        {
            return;
        }

        if (TrySpawnIssue(false))
        {
            ScheduleNextIssue();
        }
    }

    private void OnDestroy()
    {
        foreach (IssueSource source in issueSources)
        {
            if (source == null)
            {
                continue;
            }

            source.IssueActivated -= HandleIssueActivated;
            source.IssueResolved -= HandleIssueResolved;
            source.IssueFailed -= HandleIssueFailed;
        }
    }

    public float GetSystemHealth(RocketSystem system)
    {
        float health = 100f;

        foreach (IssueSource issue in activeIssues)
        {
            if (issue.IsCritical && issue.System == system)
            {
                health -= issue.CurrentHealthPenalty;
            }
        }

        health -= recoveringHealthPenalty[(int)system];

        return Mathf.Clamp(health, 0f, 100f);
    }

    public float GetTotalHealth()
    {
        float total = 
            GetSystemHealth(RocketSystem.Control) +
            GetSystemHealth(RocketSystem.Engine) +
            GetSystemHealth(RocketSystem.Electrical) +
            GetSystemHealth(RocketSystem.Fuel);

        return total / 4f;
    }

    public bool HasLaunchHealth()
    {
        return GetRoundedTotalHealth() >= RoundedRequiredLaunchHealth;
    }

    public int GetRoundedTotalHealth()
    {
        return Mathf.RoundToInt(GetTotalHealth());
    }

    public void StopSpawning()
    {
        spawningEnabled = false;
    }

    private void ScheduleNextIssue(bool useMinimumDelay = false)
    {
        float delay = useMinimumDelay
            ? minimumSpawnDelay
            : Random.Range(minimumSpawnDelay, maxSpawnDelay);
        nextSpawnTime = Time.time + delay;
    }

    private void ScheduleNextHealthTick()
    {
        bool isFinalThirtySeconds = launchManager != null &&
            launchManager.TimeRemaining <= 30f;

        float minimumDelay = isFinalThirtySeconds
            ? finalThirtyMinimumHealthTickDelay
            : minimumHealthTickDelay;
        float maximumDelay = isFinalThirtySeconds
            ? finalThirtyMaximumHealthTickDelay
            : maximumHealthTickDelay;

        float delay = Random.Range(
            minimumDelay,
            Mathf.Max(minimumDelay, maximumDelay)
        );
        nextHealthTickTime = Time.time + delay;
    }

    private void ScheduleNextHealthRecoveryTick()
    {
        float delay = Random.Range(
            minimumHealthRecoveryTickDelay,
            Mathf.Max(
                minimumHealthRecoveryTickDelay,
                maximumHealthRecoveryTickDelay
            )
        );

        nextHealthRecoveryTickTime = Time.time + delay;
    }

    private void ApplyHealthPenaltyTick()
    {
        bool healthChanged = false;

        foreach (IssueSource issue in activeIssues)
        {
            if (issue != null && issue.ApplyHealthPenaltyTick())
            {
                healthChanged = true;
            }
        }

        if (healthChanged)
        {
            RefreshBoard();
        }
    }

    private void ApplyHealthRecoveryTick()
    {
        bool healthChanged = false;

        for (int i = 0; i < recoveringHealthPenalty.Length; i++)
        {
            if (recoveringHealthPenalty[i] <= 0f)
            {
                continue;
            }

            recoveringHealthPenalty[i] = Mathf.Max(
                0f,
                recoveringHealthPenalty[i] - 1f
            );
            healthChanged = true;
        }

        if (healthChanged)
        {
            RefreshBoard();
        }
    }

    private bool TrySpawnIssue(bool inconveniencesOnly)
    {
        if (activeIssues.Count >= maxActiveIssues)
        {
            return false;
        }

        int totalWeight = 0;

        foreach (IssueSource source in issueSources)
        {
            if (source == null ||
                !source.IsAvailableForSelection ||
                (inconveniencesOnly && source.IsCritical))
            {
                continue;
            }

            totalWeight += source.SpawnWeight;
        }
        
        if (totalWeight <= 0)
        {
            return false;
        }

        int roll = Random.Range(0, totalWeight);

        foreach (IssueSource source in issueSources)
        {
            if (source == null ||
                !source.IsAvailableForSelection ||
                (inconveniencesOnly && source.IsCritical))
            {
                continue;
            }

            roll -= source.SpawnWeight;

            if (roll < 0)
            {
                source.ActivateIssue();
                return true;
            }
        } 

        return false;
    }

    private void HandleIssueActivated(IssueSource source)
    {
        if (!activeIssues.Contains(source))
        {
            activeIssues.Add(source);
        }

        RefreshBoard();
    }

    private void HandleIssueResolved(IssueSource source)
    {
        activeIssues.Remove(source);
        source.StartSelectionCooldown(completedTaskCooldown);

        if (source.IsCritical)
        {
            recoveringHealthPenalty[(int)source.System] +=
                source.LastResolvedHealthPenalty;
        }

        RefreshBoard();
    }

    private void HandleIssueFailed(IssueSource source)
    {
        if (Random.value <= failureInconvenienceChance)
        {
            TrySpawnIssue(true);
        }

        RefreshBoard();
    }

    private void RefreshBoard()
    {
        controlBoard?.Refresh(this);
    }
}
