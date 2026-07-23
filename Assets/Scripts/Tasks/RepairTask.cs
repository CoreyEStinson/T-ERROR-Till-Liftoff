using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class RepairTask : Interactable
{
    [Header("Task Information")]
    [SerializeField] private string taskName = "Repair";
    [SerializeField] private ToolType requiredTool = ToolType.None;

    [Header("Task State")]
    [SerializeField] private bool taskIsActive = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onTaskCompleted;

    public bool IsCompleted { get; private set; }
    public bool IsFocused { get; private set; }
    public bool IsTaskActive => taskIsActive;
    public string TaskName => taskName;

    public event Action<RepairTask> TaskCompleted;
    public event Action<RepairTask> TaskFailed;

    protected bool HasRequiredTool(ToolType equippedTool)
    {
        return requiredTool == ToolType.None || equippedTool == requiredTool;
    }

    public override bool UsesUniversalInteraction => false;

    public override string GetInteractionPrompt(ToolType equippedTool)
    {
        if (!taskIsActive || IsCompleted)
        {
            return string.Empty;
        }

        if (!HasRequiredTool(equippedTool))
        {
            return $"Required {requiredTool}";
        }

        return GetActivePrompt();
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return false;
    }

    // Required by Interactable
    public override void Interact(ToolType equippedTool)
    {
    }

    public override void OnFocusEnter(ToolType equippedTool)
    {
        IsFocused = true;
    }

    public override void OnFocusStay(ToolType equippedTool)
    {
        if (!taskIsActive || IsCompleted || !HasRequiredTool(equippedTool))
        {
            return;   
        }

        HandleFocusedInput();
    }

    public override void OnFocusExit()
    {
        IsFocused = false;
        HandleFocusLost();
    }

    public void ActivateTask()
    {
        IsCompleted = false;
        taskIsActive = true;
        OnTaskActivated();
    }

    public void DeactivateTask()
    {
        taskIsActive = false;
        IsFocused = false;
        OnTaskDeactivated();
    }

    protected void CompleteTask()
    {
        if (IsCompleted)
        {
            return;
        }

        IsCompleted = true;
        TaskCompleted?.Invoke(this);
        onTaskCompleted?.Invoke();
    }

    protected void FailTask()
    {
        TaskFailed?.Invoke(this);
    }

    protected virtual void OnTaskActivated() { }

    protected virtual void OnTaskDeactivated() { }

    // Each task supplies its own player-facing instruction.
    protected abstract string GetActivePrompt();

    // Each task handles its own physical input here.
    protected abstract void HandleFocusedInput();

    // Override this only if a task needs to reset/cancel partial input on look-away.
    protected virtual void HandleFocusLost() { }
}