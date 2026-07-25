using System.Collections;
using TMPro;
using UnityEngine;

public class RollingCountdownText : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text currentText;
    [SerializeField] private TMP_Text incomingText;

    [Header("Animation")]
    [SerializeField] private float rollDuration = 0.18f;
    [SerializeField] private float rollDistance = 65f;

    private RectTransform currentRect;
    private RectTransform incomingRect;
    private Coroutine rollRoutine;
    private string displayedValue;
    private bool isInitialized;

    private void Awake()
    {
        if (currentText != null)
        {
            currentRect = currentText.rectTransform;
        }

        if (incomingText != null)
        {
            incomingRect = incomingText.rectTransform;
        }
    }

    public void SetCountdown(float secondsRemaining)
    {
        int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(secondsRemaining));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        string nextValue = $"T-MINUS {minutes:00}:{seconds:00}";

        if (!isInitialized)
        {
            SetImmediately(nextValue);
            return;
        }

        if (nextValue == displayedValue)
        {
            return;
        }

        if (rollRoutine != null)
        {
            StopCoroutine(rollRoutine);
        }

        rollRoutine = StartCoroutine(RollTo(nextValue));
    }

    private void SetImmediately(string value)
    {
        displayedValue = value;
        isInitialized = true;

        currentText.text = value;
        incomingText.text = string.Empty;

        currentRect.anchoredPosition = Vector2.zero;
        incomingRect.anchoredPosition = new Vector2(0f, rollDistance);
    }

    private IEnumerator RollTo(string nextValue)
    {
        incomingText.text = nextValue;

        currentRect.anchoredPosition = Vector2.zero;
        incomingRect.anchoredPosition = new Vector2(0f, rollDistance);

        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / rollDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Old countdown moves down and disappears.
            currentRect.anchoredPosition =
                new Vector2(0f, Mathf.Lerp(0f, -rollDistance, easedProgress));

            // New countdown drops down from above.
            incomingRect.anchoredPosition =
                new Vector2(0f, Mathf.Lerp(rollDistance, 0f, easedProgress));

            yield return null;
        }

        currentText.text = nextValue;
        currentRect.anchoredPosition = Vector2.zero;

        incomingText.text = string.Empty;
        incomingRect.anchoredPosition = new Vector2(0f, rollDistance);

        displayedValue = nextValue;
        rollRoutine = null;
    }
}