using UnityEngine;

public class WindowLightBeams : MonoBehaviour
{
    [System.Serializable]
    private struct ControlledBeam
    {
        public SpriteRenderer renderer;
        [Min(0f)] public float moonLength;
        [Min(0f)] public float sunriseLength;
        [Min(0.01f)] public float thickness;
        [Range(0f, 2f)] public float alphaMultiplier;
    }

    [Header("Clock")]
    [SerializeField] private HudClock hudClock;

    [Header("Controlled Beams")]
    [SerializeField] private ControlledBeam[] controlledBeams;

    [Header("Dawn Timing")]
    [SerializeField, Range(0, 23)] private int fasterChangeStartHour = 4;
    [SerializeField, Range(0, 59)] private int fasterChangeStartMinute;
    [SerializeField, Range(0f, 1f)] private float beamProgressAtFasterChangeStart = 0.25f;

    [Header("Direction")]
    [SerializeField] private float moonAngle = -42f;
    [SerializeField] private float sunriseAngle = -14f;

    [Header("Color")]
    [SerializeField] private Color moonColor = new Color(0.45f, 0.5f, 1f, 0.16f);
    [SerializeField] private Color sunriseColor = new Color(1f, 0.68f, 0.28f, 0.5f);

    private bool stoppedAtEnd;

    private void Awake()
    {
        if (hudClock == null)
        {
            hudClock = FindFirstObjectByType<HudClock>();
        }
    }

    private void Start()
    {
        ApplyBeams();
    }

    private void Update()
    {
        if (hudClock == null)
        {
            return;
        }

        if (stoppedAtEnd)
        {
            if (hudClock.Progress01 < 0.999f)
            {
                stoppedAtEnd = false;
            }
            else
            {
                return;
            }
        }

        ApplyBeams();

        if (hudClock.Progress01 >= 0.999f)
        {
            stoppedAtEnd = true;
        }
    }

    private void OnValidate()
    {
        fasterChangeStartHour = Mathf.Clamp(fasterChangeStartHour, 0, 23);
        fasterChangeStartMinute = Mathf.Clamp(fasterChangeStartMinute, 0, 59);
        beamProgressAtFasterChangeStart = Mathf.Clamp01(beamProgressAtFasterChangeStart);

        if (moonColor.a <= 0f)
        {
            moonColor.a = 0.16f;
        }

        if (sunriseColor.a <= 0f)
        {
            sunriseColor.a = 0.5f;
        }
    }

    private void ApplyBeams()
    {
        if (hudClock == null || controlledBeams == null)
        {
            return;
        }

        float progress = GetBeamProgress(hudClock.Progress01);
        float angle = Mathf.LerpAngle(moonAngle, sunriseAngle, progress);
        Color beamColor = Color.Lerp(moonColor, sunriseColor, progress);

        for (int i = 0; i < controlledBeams.Length; i++)
        {
            ControlledBeam beam = controlledBeams[i];
            if (beam.renderer == null)
            {
                continue;
            }

            float length = Mathf.Lerp(beam.moonLength, beam.sunriseLength, progress);
            float thickness = Mathf.Max(0.01f, beam.thickness);

            Transform beamTransform = beam.renderer.transform;
            beamTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            beamTransform.localScale = new Vector3(length, thickness, 1f);

            Color finalColor = beamColor;
            finalColor.a *= beam.alphaMultiplier;
            beam.renderer.color = finalColor;
        }
    }

    private float GetBeamProgress(float clockProgress)
    {
        float clampedProgress = Mathf.Clamp01(clockProgress);
        float fasterChangeStartProgress = Mathf.Clamp(hudClock.GetProgressAtTime(fasterChangeStartHour, fasterChangeStartMinute), 0.01f, 0.99f);
        float beamProgressAtStart = Mathf.Clamp(beamProgressAtFasterChangeStart, 0.01f, 0.99f);

        if (clampedProgress <= fasterChangeStartProgress)
        {
            float earlyProgress = clampedProgress / fasterChangeStartProgress;
            return Mathf.Lerp(0f, beamProgressAtStart, Mathf.SmoothStep(0f, 1f, earlyProgress));
        }

        float dawnProgress = (clampedProgress - fasterChangeStartProgress) / (1f - fasterChangeStartProgress);
        return Mathf.Lerp(beamProgressAtStart, 1f, Mathf.SmoothStep(0f, 1f, dawnProgress));
    }
}
