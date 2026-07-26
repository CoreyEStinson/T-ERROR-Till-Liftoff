using UnityEngine;

[RequireComponent(typeof(Light))]
public class DirectionalLightAlarm : MonoBehaviour
{
    private enum AlarmMode
    {
        SmoothPulse,
        InstantFlash
    }

    [Header("Light")]
    [SerializeField] private Light alarmLight;
    [SerializeField, Min(0f)] private float maximumIntensity = 1f;

    [Header("Alarm Behaviour")]
    [SerializeField] private AlarmMode mode = AlarmMode.SmoothPulse;
    [Tooltip("Seconds for one full off-to-on-to-off cycle.")]
    [SerializeField, Min(0.01f)] private float cycleDuration = 1f;

    private float alarmTime;

    private void Awake()
    {
        if (alarmLight == null)
        {
            alarmLight = GetComponent<Light>();
        }
    }

    private void OnEnable()
    {
        alarmTime = 0f;
    }

    private void Update()
    {
        if (alarmLight == null)
        {
            return;
        }

        alarmTime += Time.deltaTime;

        float cycleProgress = Mathf.Repeat(alarmTime / cycleDuration, 1f);

        if (mode == AlarmMode.SmoothPulse)
        {
            float pulse = Mathf.PingPong(cycleProgress * 2f, 1f);
            alarmLight.intensity = pulse * maximumIntensity;
        }
        else
        {
            alarmLight.intensity = cycleProgress < 0.5f ? maximumIntensity : 0f;
        }
    }

    private void OnValidate()
    {
        cycleDuration = Mathf.Max(0.01f, cycleDuration);
        maximumIntensity = Mathf.Max(0f, maximumIntensity);
    }
}
