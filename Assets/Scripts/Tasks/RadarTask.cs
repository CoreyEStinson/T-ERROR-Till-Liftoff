using System.Text;
using TMPro;
using UnityEngine;

public class RadarTask : RepairTask
{
    [Header("Radar Dials")]
    [SerializeField] private RadarDial[] radarDials;
    [SerializeField, Min(2)] private int dialPositions = 8;
    [SerializeField] private TMP_Text calibrationReadout;
    [TextArea]
    [SerializeField] private string inactiveReadout =
        "RADAR CALIBRATION\nSYSTEM STANDBY\n\nNO CALIBRATION REQUIRED";

    protected override string GetActivePrompt()
    {
        return "Align all radar dials";
    }

    protected override void HandleFocusedInput() { }

    public bool CanAdjustDial(RadarDial dial, ToolType equippedTool)
    {
        return IsTaskActive &&
            !IsCompleted &&
            HasRequiredTool(equippedTool) &&
            System.Array.IndexOf(radarDials, dial) >= 0;
    }

    public void AdjustDial(RadarDial dial, ToolType equippedTool)
    {
        if (!CanAdjustDial(dial, equippedTool))
        {
            return;
        }

        dial.Advance(dialPositions);
        UpdateReadout();

        if (AllDialsAligned())
        {
            CompleteTask();
        }
    }

    protected override void OnTaskActivated()
    {
        if (radarDials == null || radarDials.Length == 0)
        {
            Debug.LogWarning($"{name} needs RadarDial references.", this);
            return;
        }

        foreach (RadarDial dial in radarDials)
        {
            if (dial == null)
            {
                continue;
            }

            int target = Random.Range(0, dialPositions);
            int starting = Random.Range(1, dialPositions);
            starting = (target + starting) % dialPositions;
            dial.Configure(starting, target, dialPositions);
        }

        UpdateReadout();
    }

    protected override void OnTaskDeactivated()
    {
        if (calibrationReadout != null)
        {
            calibrationReadout.text = inactiveReadout;
        }
    }

    private bool AllDialsAligned()
    {
        foreach (RadarDial dial in radarDials)
        {
            if (dial == null || !dial.IsAligned)
            {
                return false;
            }
        }

        return true;
    }

    private void UpdateReadout()
    {
        if (calibrationReadout == null || radarDials == null)
        {
            return;
        }

        StringBuilder readout = new StringBuilder(
            "RADAR CALIBRATION\nSIGNAL LOCK REQUIRED\n\n"
        );

        for (int i = 0; i < radarDials.Length; i++)
        {
            RadarDial dial = radarDials[i];

            if (dial != null)
            {
                readout.AppendLine($"DIAL {i + 1}: " +
                    $"{dial.CurrentStep + 1:00} / {dial.TargetStep + 1:00}");
            }
        }

        readout.Append("\nALIGN ALL DIALS");

        calibrationReadout.text = readout.ToString();
    }
}
