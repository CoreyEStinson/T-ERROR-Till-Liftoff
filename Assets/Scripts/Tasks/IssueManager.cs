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
    [SerializeField, Range(0f, 1f)] private float failureInconvenienceChance = 0.15f;

    [Header("Board")]
    [SerializeField] private ControlBoard controlBoard;

    private readonly List<IssueSource> activeIssues = new();
    private float nextSpawnTime;

    public IReadOnlyList<IssueSource> ActiveIssues => activeIssues;

    private void Awake()
    {
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

        ScheduleNextIssue();
        RefreshBoard();
    }

    private void Update()
    {
        if (Time.time < nextSpawnTime || activeIssues.Count >= maxActiveIssues)
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
                health -= issue.HealthPenalty;
            }
        }
        
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
        return GetTotalHealth() > 90f;
    }

    private void ScheduleNextIssue()
    {
        float delay = Random.Range(minimumSpawnDelay, maxSpawnDelay);
        nextSpawnTime = Time.time + delay;
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
                source.IsActive ||
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
                source.IsActive ||
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