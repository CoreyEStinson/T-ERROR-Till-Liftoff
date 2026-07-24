using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BreakerTask: RepairTask
{
    [Header("Breaker References")]
    [SerializeField] private BreakerLever[] breakers;

    [Header("Box Visuals")]
    [SerializeField] private GameObject closedBoxVisual;
    [SerializeField] private GameObject openBoxVisual;

    [Header("Sequence Settings")]
    [SerializeField, Min(2)] private int sequenceLength = 3;
    [SerializeField] private float initialSequenceDelay = 0.5f;
    [SerializeField] private float indicatorOnDuration = 0.45f;
    [SerializeField] private float gapBetweenIndicators = 0.2f;
    [SerializeField] private float failurePause = 0.6f;

    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceFailed;

    private int[] sequence;
    private bool[] leverUsedThisCycle;
    private int currentInputIndex;
    private bool taskStarted;
    private bool acceptingInput;
    private Coroutine sequenceRoutine;

    public override bool UsesUniversalInteraction => true;

    public bool CanUseLevers =>
        IsTaskActive &&
        !IsCompleted &&
        taskStarted &&
        acceptingInput;

    protected override string GetActivePrompt()
    {
        if (!taskStarted)
        {
            return "[E] Open breaker box";
        }

        if (!acceptingInput)
        {
            return "Watch the indicator lights";
        }

        return "Select the breaker sequence";
    }

    public override bool CanInteract(ToolType equippedTool)
    {
        return IsTaskActive && !IsCompleted && !taskStarted;
    }

    public override void Interact(ToolType equippedTool)
    {
        if (!CanInteract(equippedTool))
        {
            return;
        }

        taskStarted = true;
        SetBoxOpen(true);

        GenerateSequence();
        StartSequence(false);
    }

    public bool CanUseLever(BreakerLever selectedBreaker)
    {
        if (!CanUseLevers)
        {
            return false;
        }

        int index = GetBreakerIndex(selectedBreaker);
        
        return index >= 0 && !leverUsedThisCycle[index];
    }

    public void TryFlipLever(BreakerLever selectedBreaker)
    {
        if (!CanUseLever(selectedBreaker))
        {
            return;
        }

        int selectedIndex = GetBreakerIndex(selectedBreaker);

        // A lever may only be selected once this attempt
        leverUsedThisCycle[selectedIndex] = true;
        selectedBreaker.SetFlipped(true);

        if (selectedIndex == sequence[currentInputIndex])
        {
            currentInputIndex++;

            if (currentInputIndex >= sequence.Length)
            {
                acceptingInput = false;
                StartCoroutine(CompleteAfterDelay(1f));
            }

            return;
        }

        acceptingInput = false;
        FailTask();
        onSequenceFailed?.Invoke();

        GenerateSequence();
        StartSequence(true);
    }

    private IEnumerator CompleteAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        CompleteTask();
    }

    protected override void OnTaskActivated()
    {
        StopSequence();

        taskStarted = false;
        acceptingInput = false;

        ResetPanelVisuals();
        SetBoxOpen(false);
    }

    protected override void OnTaskDeactivated()
    {
        StopSequence();
        taskStarted = false;
        acceptingInput = false;

        ResetPanelVisuals();
        SetBoxOpen(false);
    }

    private void GenerateSequence()
    {
        if (breakers == null || breakers.Length < 2)
        {
            Debug.LogWarning(
                $"{name} needs at least two BreakerLevers assigned", 
                this
            );

            return;
        }

        // A lever can only occur once
        int actualSequenceLength = Mathf.Min(sequenceLength, breakers.Length);

        sequence = new int[actualSequenceLength];
        List<int> availiableIndices = new List<int>();

        for (int i = 0; i < breakers.Length; i++)
        {
            availiableIndices.Add(i);
        }

        for (int i = 0; i < sequence.Length; i++)
        {
            int randomListIndex = Random.Range(0, availiableIndices.Count);

            sequence[i] = availiableIndices[randomListIndex];
            availiableIndices.RemoveAt(randomListIndex);
        }
    }

    private void StartSequence(bool afterFailure)
    {
        StopSequence();
        sequenceRoutine = StartCoroutine(ShowSequence(afterFailure));
    }

    private void StopSequence()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }
    }

    private IEnumerator ShowSequence(bool afterFailure)
    {
        acceptingInput = false;
        currentInputIndex = 0;

        leverUsedThisCycle = new bool[breakers.Length];
        ResetPanelVisuals();

        yield return new WaitForSeconds(
            afterFailure ? failurePause : initialSequenceDelay);

        foreach (int breakerIndex in sequence)
        {
            BreakerLever breaker = breakers[breakerIndex];

            if (breaker == null)
            {
                continue;
            }

            breaker.SetSequenceIndicator(true);

            yield return new WaitForSeconds(indicatorOnDuration);

            breaker.SetSequenceIndicator(false);

            yield return new WaitForSeconds(gapBetweenIndicators);
        }

        acceptingInput = true;
        sequenceRoutine = null;
    }

    private void ResetPanelVisuals()
    {
        if (breakers == null)
        {
            return;
        }

        foreach (BreakerLever breaker in breakers)
        {
            if (breaker == null)
            {
                continue;
            }

            breaker.SetFlipped(false);
            breaker.SetSequenceIndicator(false);
        }
    }

    private void SetBoxOpen(bool isOpen)
    {
        closedBoxVisual?.SetActive(!isOpen);

        openBoxVisual?.SetActive(isOpen);
    }

    private int GetBreakerIndex(BreakerLever breakerToFind)
    {
        for (int i = 0; i < breakers.Length; i++)
        {
            if (breakers[i] == breakerToFind)
            {
                return i;
            }
        }

        return -1;
    }

    protected override void HandleFocusedInput() { }
}