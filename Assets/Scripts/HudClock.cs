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
    [SerializeField] private int displayMinuteStep = 10;

    [Header("Clock Color")]
    [SerializeField] private bool useCurrentTextColorAsStart = true;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.red;

    [Header("Clock Outline")]
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField, Range(0f, 1f)] private float outlineWidth = 0.15f;

    private float elapsedSeconds;
    private Color runtimeStartColor;

    private void Awake()
    {
        if (clockText == null)
        {
            clockText = GetComponent<TMP_Text>();
        }

        PrepareClockText();
    }

    private void OnValidate()
    {
        durationMinutes = Mathf.Max(0.01f, durationMinutes);
        if (displayMinuteStep <= 0)
        {
            displayMinuteStep = 10;
        }
        FixTransparentDefaultColor(ref startColor, Color.white);
        FixTransparentDefaultColor(ref endColor, Color.red);
        FixTransparentDefaultColor(ref outlineColor, Color.black);
        outlineWidth = Mathf.Clamp01(outlineWidth);
    }

    private void Start()
    {
        PrepareClockText();
        CaptureStartColor();
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

    public void SetDurationMinutes(float minutes)
    {
        durationMinutes = Mathf.Max(0.01f, minutes);
        UpdateClockText();
    }

    private void PrepareClockText()
    {
        if (clockText == null)
        {
            return;
        }

        clockText.enableVertexGradient = false;
        ApplyClockOutline();
    }

    private void CaptureStartColor()
    {
        runtimeStartColor = useCurrentTextColorAsStart && clockText != null
            ? clockText.color
            : startColor;

        FixTransparentDefaultColor(ref runtimeStartColor, Color.white);
        FixTransparentDefaultColor(ref endColor, Color.red);
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
        currentTotalMinutes = RoundDownToMinuteStep(currentTotalMinutes, displayMinuteStep);

        currentTotalMinutes %= 24 * 60;
        int hour = currentTotalMinutes / 60;
        int minute = currentTotalMinutes % 60;

        clockText.text = use24HourClock
            ? $"{hour:00}:{minute:00}"
            : Format12HourTime(hour, minute);

        ApplyClockColor(Color.Lerp(runtimeStartColor, endColor, progress));
    }

    private void ApplyClockColor(Color color)
    {
        color.a = 1f;
        clockText.color = color;
        clockText.faceColor = color;
        ApplyClockOutline();
    }

    private void ApplyClockOutline()
    {
        Color visibleOutlineColor = outlineColor;
        FixTransparentDefaultColor(ref visibleOutlineColor, Color.black);

        clockText.outlineColor = visibleOutlineColor;
        clockText.outlineWidth = outlineWidth;
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

    private int RoundDownToMinuteStep(int totalMinutes, int minuteStep)
    {
        int safeStep = Mathf.Max(1, minuteStep);
        return totalMinutes - totalMinutes % safeStep;
    }

    private void FixTransparentDefaultColor(ref Color color, Color fallbackColor)
    {
        if (color.a > 0f)
        {
            return;
        }

        color = color.r == 0f && color.g == 0f && color.b == 0f
            ? fallbackColor
            : new Color(color.r, color.g, color.b, 1f);
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
