using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LaunchManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IssueManager issueManager;
    [SerializeField] private ControlBoard controlBoard;
    [SerializeField] private FirstPersonController playerController;

    [Header("Countdown")]
    [SerializeField, Min(1)] private float launchDurationSeconds = 300f;
    [SerializeField] private TMP_Text finalCountdownHud;
    [SerializeField] private TMP_Text resultHud;

    [Header("Audio")]
    [SerializeField] private AudioSource alarmSource;
    [SerializeField] private AudioSource voiceSound;
    [SerializeField] private AudioClip threeMinuteWarning;
    [SerializeField] private AudioClip twoMinuteWarning;
    [SerializeField] private AudioClip oneMinuteWarning;
    [SerializeField] private AudioClip thirtySecondWarning;
    [SerializeField] private AudioClip finalTenSeconds;

    [Header("Thirty Second Alarms")]
    [SerializeField] private Light mainLight;
    [SerializeField] private DirectionalLightAlarm[] thirtySecondAlarmLights;

    [Header("Result Text")]
    [SerializeField] private string successMessage = 
        "LIFTOFF CONFIRMED - Please ignore the noises.";

    [SerializeField] private string failureMessage = "LAUNCH FAILURE";

    [Header("Launch Events")]
    [SerializeField] private UnityEvent onLaunchSuccess;
    [SerializeField] private UnityEvent onLaunchFailure;

    [Header("Menu")]
    [SerializeField] private string mainMenuScene = "MainMenu";

    private float timeRemaining;
    private bool launchFinished;
    private int lastFinalSecond = -1;
    private bool threeMinutePlayed;
    private bool twoMinutePlayed;
    private bool oneMinutePlayed;
    private bool thirtySecondPlayed;
    private bool finalTenSecondsPlayed;
    private bool thirtySecondAlarmsActive;
    private bool finalTenScreenShakeActive;

    public float TimeRemaining => timeRemaining;

    private void Awake()
    {
        timeRemaining = launchDurationSeconds;

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }
    }

    private void Start()
    {
        finalCountdownHud?.gameObject.SetActive(false);
        resultHud?.gameObject.SetActive(false);
        SetThirtySecondAlarms(false);

        UpdateCountdownDisplays();
    }

    private void Update()
    {
        if (launchFinished)
        {
            CheckForMainMenuInput();
            return;
        }

        float previousTime = timeRemaining;
        timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);

        UpdateCountdownDisplays();
        PlayCountdownWarnings(previousTime);

        if (timeRemaining <= 0f)
        {
            FinishLaunch();
        }
    }

    private void UpdateCountdownDisplays()
    {
        controlBoard?.SetCountdown(timeRemaining);

        bool isFinalThirtySeconds = timeRemaining <= 30f && !launchFinished;

        if (isFinalThirtySeconds && !thirtySecondAlarmsActive)
        {
            if (mainLight != null)
            {
                mainLight.gameObject.SetActive(false);
            }

            SetThirtySecondAlarms(true);
        }

        if (timeRemaining <= 10f && !finalTenScreenShakeActive)
        {
            finalTenScreenShakeActive = true;
            playerController?.SetScreenShake(true);
        }

        if (finalCountdownHud != null)
        {
            finalCountdownHud.gameObject.SetActive(isFinalThirtySeconds);

            if (isFinalThirtySeconds)
            {
                finalCountdownHud.text = 
                    $"{Mathf.CeilToInt(timeRemaining):00}";
            }
        }
    }

    private void SetThirtySecondAlarms(bool active)
    {
        thirtySecondAlarmsActive = active;

        if (thirtySecondAlarmLights == null)
        {
            return;
        }

        foreach (DirectionalLightAlarm alarmLight in thirtySecondAlarmLights)
        {
            if (alarmLight != null)
            {
                alarmLight.gameObject.SetActive(active);
            }
        }
    }

    private void PlayCountdownWarnings(float previousTime)
    {
        if (!threeMinutePlayed && previousTime > 180f && timeRemaining <= 180f)
        {
            threeMinutePlayed = true;
            PlayAlarm(threeMinuteWarning);
        }

        if (!twoMinutePlayed && previousTime > 120f && timeRemaining <= 120f)
        {
            twoMinutePlayed = true;
            PlayAlarm(twoMinuteWarning);
        }

        if (!oneMinutePlayed && previousTime > 60f && timeRemaining <= 60f)
        {
            oneMinutePlayed = true;
            PlayAlarm(oneMinuteWarning);
        }

        if (!thirtySecondPlayed && previousTime > 30f && timeRemaining <= 30f)
        {
            thirtySecondPlayed = true;
            PlayAlarm(thirtySecondWarning);
        }

        if (!finalTenSecondsPlayed && previousTime > 12f && timeRemaining <= 12f)
        {
            finalTenSecondsPlayed = true;
            PlayAlarm(finalTenSeconds);
        } 
        
    }

    private void PlayAlarm(AudioClip clip)
    {
        if (alarmSource != null && clip != null)
        {
            alarmSource.PlayOneShot(clip);
        }
    }

    private void FinishLaunch()
    {
        launchFinished = true;
        issueManager?.StopSpawning();
        playerController?.SetScreenShake(false);
        playerController?.SetMovementInputEnabled(false);
        playerController?.SetLookInputEnabled(false);

        finalCountdownHud?.gameObject.SetActive(false);

        bool succeeded = issueManager != null && issueManager.HasLaunchHealth();

        if (resultHud != null)
        {
            resultHud.gameObject.SetActive(true);

            resultHud.text = succeeded 
                ? successMessage
                : failureMessage + "\n\n[M] Main Meun";
        }

        if (succeeded)
        {
            onLaunchSuccess?.Invoke();
        }
        else
        {
            onLaunchFailure?.Invoke();
        }
    }

    private void CheckForMainMenuInput()
    {
        if (Keyboard.current == null ||
            !Keyboard.current.mKey.wasPressedThisFrame)
        {
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            Debug.LogWarning(
                $"Main menu scene '{mainMenuScene}' is not in Build Settings"
            );
        }
    }
}
