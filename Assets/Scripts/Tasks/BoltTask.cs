using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BoltTask : RepairTask
{
    [Header("Bolt References")]
    [SerializeField] private BoltPoint[] bolts;
    [SerializeField] private FirstPersonController playerController;

    [Header("Task Settings")]
    [SerializeField, Min(1)] private int boltsPerIssue = 3;
    [SerializeField] private float progressPerMousePixel = 0.006f;
    [SerializeField] private int correctTurnDirection = 1;

    [Header("Failure")]
    [SerializeField] private bool reportFailureWhenReleased;

    [Header("Events")]
    [SerializeField] private UnityEvent onBoltTightened;

    private BoltPoint activeBolt;
    private bool isTightening;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }

        correctTurnDirection = correctTurnDirection >= 0 ? 1 : -1;
    }

    private void OnDisable()
    {
        StopTightening();
    }

    public bool CanWorkOnBolts(ToolType equippedTool)
    {
        return IsTaskActive &&
            !IsCompleted &&
            equippedTool == ToolType.Wrench;
    }

    public bool IsTightening(BoltPoint bolt)
    {
        return isTightening && activeBolt == bolt;
    }

    protected override string GetActivePrompt()
    {
        // Bolts provide their own prompts
        return string.Empty;
    }

    protected override void HandleFocusedInput()
    {
        // Bolt objects handle the input.
    }

    public void HandleBoltInput(BoltPoint selectedBolt, ToolType equippedTool)
    {
        if (!CanWorkOnBolts(equippedTool) || !selectedBolt.IsLoose)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        if (!isTightening)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                BeginTightening(selectedBolt);
            }

            return;
        }

        if (activeBolt != selectedBolt)
        {
            return;
        }


        if (!Mouse.current.leftButton.isPressed)
        {
            ResetCurrentBolt();
            return;
        }
        
        float mouseX = Mouse.current.delta.ReadValue().x;
        float progressChange = 
            mouseX * progressPerMousePixel * correctTurnDirection;
        
        selectedBolt.SetProgress(
            selectedBolt.Progress + progressChange,
            correctTurnDirection
        );

        if (selectedBolt.Progress >= 1f)
        {
            selectedBolt.SetTightened();
            onBoltTightened?.Invoke();

            StopTightening();

            if (AllSelectedBoltsAreTight())
            {
                CompleteTask();
            }
        }
    }

    public void HandleBoltFocusLost(BoltPoint bolt)
    {
        if (isTightening && activeBolt == bolt)
        {
            ResetCurrentBolt();
        }
    }

    protected override void OnTaskActivated()
    {
        StopTightening();
        ActivateRandomBolts();
    }

    protected override void OnTaskDeactivated()
    {
        StopTightening();
    }

    private void BeginTightening(BoltPoint bolt)
    {
        activeBolt = bolt;
        isTightening = true;

        playerController?.SetLookInputEnabled(false);
    }

    private void StopTightening()
    {
        activeBolt = null;
        isTightening = false;

        playerController?.SetLookInputEnabled(true);
    }

    private void ResetCurrentBolt()
    {
        if (activeBolt != null && activeBolt.IsLoose)
        {
            activeBolt.SetProgress(0f, correctTurnDirection);
        }

        StopTightening();

        if (reportFailureWhenReleased)
        {
            FailTask();
        }
    }

    private void ActivateRandomBolts()
    {
        if (bolts == null || bolts.Length == 0)
        {
            Debug.LogWarning($"{name} nneds BoltPoint references.", this);
            return;
        }

        foreach (BoltPoint bolt in bolts)
        {
            bolt.SetLoose(false);
        }

        List<int> avaliableIndices = new List<int>();

        for (int i = 0; i < bolts.Length; i++)
        {
            if (bolts[i] != null)
            {
                avaliableIndices.Add(i);
            }
        }    

        int amountToActivate = 
            Mathf.Min(boltsPerIssue, avaliableIndices.Count);
        
        for (int i = 0; i < amountToActivate; i++)
        {
            int randomListIndex = Random.Range(0, avaliableIndices.Count);
            int boltIndex = avaliableIndices[randomListIndex];

            bolts[boltIndex].SetLoose(true);
            avaliableIndices.RemoveAt(randomListIndex);   
        }
    }

    private bool AllSelectedBoltsAreTight()
    {
        foreach (BoltPoint bolt in bolts)
        {
            if (bolt != null && bolt.IsLoose)
            {
                return false;
            }
        }

        return true;
    }
}
