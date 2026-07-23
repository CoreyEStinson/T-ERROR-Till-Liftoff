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
            $"CONTROL: {issueManager.GetSystemHealth(RocketSystem.Control):0}%");

        SetText(
            engineHealthText,
            $"ENGINE: {issueManager.GetSystemHealth(RocketSystem.Engine):0}%");

        SetText(
            electricalHealthText,
            $"ELECTRICAL: {issueManager.GetSystemHealth(RocketSystem.Electrical):0}%");

        SetText(
            fuelHealthText,
            $"FUEL: {issueManager.GetSystemHealth(RocketSystem.Fuel):0}%");

        if (activeAlertsText == null)
        {
            return;
        }

        if (issueManager.ActiveIssues.Count == 0)
        {
            activeAlertsText.text = "ACTIVE ALERTS:\nNone";
        }

        StringBuilder alerts = new StringBuilder("ACTIVE ALERTS:\n");

        foreach (IssueSource issue in issueManager.ActiveIssues)
        {
            string severity = issue.IsCritical ? "CRITICAL" : "NOTICE";

            alerts.AppendLine(
                $"[{severity}] {issue.System} - {issue.AlertText}");
            alerts.AppendLine($"Room: {issue.RoomName}");
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
}