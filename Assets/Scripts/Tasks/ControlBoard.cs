using System.Text;
using TMPro;
using UnityEngine;

public class ControlBoard : MonoBehaviour
{
    [Header("Health Display")] 
    [SerializeField] private TMP_Text totalHealthText;
    [SerializeField] private TMP_Text controlHealthText;
    [SerializeField] private TMP_Text engineHealthText;
    [SerializeField] private TMP_Text electricalHealthText;
    [SerializeField] private TMP_Text fuelHealthText;
    [SerializeField] private TMP_Text countdownText;

    [Header("Alerts")]
    [SerializeField] private TMP_Text activeAlertsText;

    public void Refresh(IssueManager issueManager)
    {
        if (issueManager == null)
        {
            return;
        }

        SetText(
            totalHealthText,
            $"TOTAL INTEGRITY: {issueManager.GetTotalHealth():0}%");

        SetText(
            controlHealthText,
            $"CONTROL:\n{issueManager.GetSystemHealth(RocketSystem.Control):0}%");

        SetText(
            engineHealthText,
            $"ENGINE:\n{issueManager.GetSystemHealth(RocketSystem.Engine):0}%");

        SetText(
            electricalHealthText,
            $"ELECTRICAL:\n{issueManager.GetSystemHealth(RocketSystem.Electrical):0}%");

        SetText(
            fuelHealthText,
            $"FUEL:\n{issueManager.GetSystemHealth(RocketSystem.Fuel):0}%");

        if (activeAlertsText == null)
        {
            return;
        }

        if (issueManager.ActiveIssues.Count == 0)
        {
            activeAlertsText.text = "None";
            return;
        }

        StringBuilder alerts = new StringBuilder();

        foreach (IssueSource issue in issueManager.ActiveIssues)
        {
            string severity = issue.IsCritical ? "CRITICAL" : "NOTICE";

            alerts.AppendLine(
                $"[{severity}] {issue.System} - {issue.AlertText}");
            alerts.AppendLine($"Room: {issue.RoomName}\n");
        }

        activeAlertsText.text = alerts.ToString();
    }

    private void SetText(TMP_Text textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }

    public void SetCountdown(float secondsRemaining)
    {
        if (countdownText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(secondsRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        countdownText.text = $"T-MINUS {minutes:00}:{seconds:00}";
    }
}