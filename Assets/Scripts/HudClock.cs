using TMPro;
using UnityEngine;

public class HudClock : MonoBehaviour
{
    [Header("Clock Text")]
    [SerializeField] private TMP_Text clockText;

    [Header("Game Time")]
    [SerializeField] private int startHour = 22;
    [SerializeField] private int startMinute;
    [SerializeField] private int endHour = 6;
    [SerializeField] private int endMinute;

    [Header("Real Time")]
    public float durationMinutes = 2f;
    [SerializeField] private bool loopWhenFinished;
    [SerializeField] private bool useUnscaledTime;

    [Header("Display")]
    [SerializeField] private bool use24HourClock;

    private float elapsedSeconds;

    private void Awake()
    {
        if (clockText == null)
        {
            clockText = GetComponent<TMP_Text>();
        }
    }

    private void Start()
    {
        UpdateClockText();
    }

    private void Update()
    {
        float durationSeconds = Mathf.Max(1f, durationMinutes * 60f);
        elapsedSeconds += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (loopWhenFinished)
        {
            elapsedSeconds %= durationSeconds;
        }
        else
        {
            elapsedSeconds = Mathf.Min(elapsedSeconds, durationSeconds);
        }

        UpdateClockText();
    }

    public void ResetClock()
    {
        elapsedSeconds = 0f;
        UpdateClockText();
    }

    private void UpdateClockText()
    {
        if (clockText == null)
        {
            return;
        }

        int totalNightMinutes = GetMinutesBetween(startHour, startMinute, endHour, endMinute);
        float durationSeconds = Mathf.Max(1f, durationMinutes * 60f);
        float progress = Mathf.Clamp01(elapsedSeconds / durationSeconds);
        int currentTotalMinutes = GetStartTotalMinutes() + Mathf.FloorToInt(totalNightMinutes * progress);

        currentTotalMinutes %= 24 * 60;
        int hour = currentTotalMinutes / 60;
        int minute = currentTotalMinutes % 60;

        clockText.text = use24HourClock
            ? $"{hour:00}:{minute:00}"
            : Format12HourTime(hour, minute);
    }

    private int GetStartTotalMinutes()
    {
        return Mathf.Clamp(startHour, 0, 23) * 60 + Mathf.Clamp(startMinute, 0, 59);
    }

    private int GetMinutesBetween(int fromHour, int fromMinute, int toHour, int toMinute)
    {
        int from = Mathf.Clamp(fromHour, 0, 23) * 60 + Mathf.Clamp(fromMinute, 0, 59);
        int to = Mathf.Clamp(toHour, 0, 23) * 60 + Mathf.Clamp(toMinute, 0, 59);
        int minutes = to - from;

        if (minutes <= 0)
        {
            minutes += 24 * 60;
        }

        return minutes;
    }

    private string Format12HourTime(int hour, int minute)
    {
        string suffix = hour < 12 ? "AM" : "PM";
        int displayHour = hour % 12;

        if (displayHour == 0)
        {
            displayHour = 12;
        }

        return $"{displayHour}:{minute:00} {suffix}";
    }
}
