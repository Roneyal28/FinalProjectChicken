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

    [Header("Chicken Beam Lighting")]
    [SerializeField] private SpriteRenderer chickenRenderer;
    [SerializeField] private Color chickenMoonTint = new Color(0.72f, 0.82f, 1.15f, 1f);
    [SerializeField] private Color chickenSunriseTint = new Color(1.35f, 1.08f, 0.68f, 1f);
    [SerializeField, Range(0f, 1f)] private float chickenMoonTintStrength = 0.75f;
    [SerializeField, Range(0f, 1f)] private float chickenSunriseTintStrength = 0.45f;
    [SerializeField, Min(0f)] private float chickenMoonBrightnessBoost = 0.55f;
    [SerializeField, Min(0f)] private float chickenSunriseBrightnessBoost = 0.35f;
    [SerializeField, Min(0f)] private float chickenLightFadeSpeed = 9f;

    private bool stoppedAtEnd;
    private Color chickenBaseColor = Color.white;
    private float currentChickenLightStrength;
    private float lastBeamProgress;

    private void Awake()
    {
        if (hudClock == null)
        {
            hudClock = FindFirstObjectByType<HudClock>();
        }

        FindChickenRendererIfNeeded();
        CaptureChickenBaseColor();
    }

    private void Start()
    {
        ApplyBeams();
        ApplyChickenLighting();
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

    private void LateUpdate()
    {
        ApplyChickenLighting();
    }

    private void OnValidate()
    {
        fasterChangeStartHour = Mathf.Clamp(fasterChangeStartHour, 0, 23);
        fasterChangeStartMinute = Mathf.Clamp(fasterChangeStartMinute, 0, 59);
        beamProgressAtFasterChangeStart = Mathf.Clamp01(beamProgressAtFasterChangeStart);
        chickenMoonTintStrength = Mathf.Clamp01(chickenMoonTintStrength);
        chickenSunriseTintStrength = Mathf.Clamp01(chickenSunriseTintStrength);
        chickenMoonBrightnessBoost = Mathf.Max(0f, chickenMoonBrightnessBoost);
        chickenSunriseBrightnessBoost = Mathf.Max(0f, chickenSunriseBrightnessBoost);
        chickenLightFadeSpeed = Mathf.Max(0f, chickenLightFadeSpeed);

        if (moonColor.a <= 0f)
        {
            moonColor.a = 0.16f;
        }

        if (sunriseColor.a <= 0f)
        {
            sunriseColor.a = 0.5f;
        }

        if (chickenMoonTint.a <= 0f)
        {
            chickenMoonTint.a = 1f;
        }

        if (chickenSunriseTint.a <= 0f)
        {
            chickenSunriseTint.a = 1f;
        }
    }

    private void ApplyBeams()
    {
        if (hudClock == null || controlledBeams == null)
        {
            return;
        }

        lastBeamProgress = GetBeamProgress(hudClock.Progress01);
        float angle = Mathf.LerpAngle(moonAngle, sunriseAngle, lastBeamProgress);
        Color beamColor = Color.Lerp(moonColor, sunriseColor, lastBeamProgress);

        for (int i = 0; i < controlledBeams.Length; i++)
        {
            ControlledBeam beam = controlledBeams[i];
            if (beam.renderer == null)
            {
                continue;
            }

            float length = Mathf.Lerp(beam.moonLength, beam.sunriseLength, lastBeamProgress);
            float thickness = Mathf.Max(0.01f, beam.thickness);

            Transform beamTransform = beam.renderer.transform;
            beamTransform.rotation = Quaternion.Euler(0f, 0f, angle);
            beamTransform.localScale = new Vector3(length, thickness, 1f);

            Color finalColor = beamColor;
            finalColor.a *= beam.alphaMultiplier;
            beam.renderer.color = finalColor;
        }
    }

    private void ApplyChickenLighting()
    {
        FindChickenRendererIfNeeded();
        if (chickenRenderer == null)
        {
            return;
        }

        float targetStrength = GetStrongestChickenBeamOverlap();
        float fadeSpeed = chickenLightFadeSpeed <= 0f ? 1f : chickenLightFadeSpeed;
        currentChickenLightStrength = Mathf.MoveTowards(currentChickenLightStrength, targetStrength, fadeSpeed * Time.deltaTime);

        Color timeTint = Color.Lerp(chickenMoonTint, chickenSunriseTint, lastBeamProgress);
        Color litColor = MultiplyColor(chickenBaseColor, timeTint);
        float tintStrength = Mathf.Lerp(chickenMoonTintStrength, chickenSunriseTintStrength, lastBeamProgress);
        float brightnessBoost = Mathf.Lerp(chickenMoonBrightnessBoost, chickenSunriseBrightnessBoost, lastBeamProgress);
        float brightness = 1f + brightnessBoost * currentChickenLightStrength;
        litColor.r *= brightness;
        litColor.g *= brightness;
        litColor.b *= brightness;
        litColor.a = chickenBaseColor.a;

        chickenRenderer.color = Color.Lerp(chickenBaseColor, litColor, currentChickenLightStrength * tintStrength);
    }

    private float GetStrongestChickenBeamOverlap()
    {
        if (controlledBeams == null)
        {
            return 0f;
        }

        Bounds chickenBounds = chickenRenderer.bounds;
        Vector3 center = chickenBounds.center;
        Vector3 min = chickenBounds.min;
        Vector3 max = chickenBounds.max;
        Vector3[] samplePoints =
        {
            center,
            new Vector3(min.x, center.y, center.z),
            new Vector3(max.x, center.y, center.z),
            new Vector3(center.x, min.y, center.z),
            new Vector3(center.x, max.y, center.z),
        };

        float strongestOverlap = 0f;
        for (int i = 0; i < controlledBeams.Length; i++)
        {
            ControlledBeam beam = controlledBeams[i];
            if (beam.renderer == null || beam.renderer.sprite == null || !beam.renderer.enabled)
            {
                continue;
            }

            for (int pointIndex = 0; pointIndex < samplePoints.Length; pointIndex++)
            {
                float overlap = GetBeamOverlapAtPoint(beam, samplePoints[pointIndex]);
                strongestOverlap = Mathf.Max(strongestOverlap, overlap);
            }
        }

        return Mathf.Clamp01(strongestOverlap);
    }

    private float GetBeamOverlapAtPoint(ControlledBeam beam, Vector3 worldPoint)
    {
        Bounds spriteBounds = beam.renderer.sprite.bounds;
        Vector3 localPoint = beam.renderer.transform.InverseTransformPoint(worldPoint);

        if (localPoint.x < spriteBounds.min.x || localPoint.x > spriteBounds.max.x ||
            localPoint.y < spriteBounds.min.y || localPoint.y > spriteBounds.max.y)
        {
            return 0f;
        }

        float length01 = Mathf.InverseLerp(spriteBounds.min.x, spriteBounds.max.x, localPoint.x);
        float width01 = Mathf.InverseLerp(spriteBounds.min.y, spriteBounds.max.y, localPoint.y);
        float centerWidth = 1f - Mathf.Abs(width01 - 0.5f) * 2f;
        float softWidth = Mathf.SmoothStep(0f, 1f, centerWidth);
        float softLength = Mathf.Lerp(1f, 0.35f, length01);
        float visibleAlpha = beam.renderer.color.a / Mathf.Max(0.001f, Mathf.Max(moonColor.a, sunriseColor.a));

        return softWidth * softLength * beam.alphaMultiplier * visibleAlpha;
    }

    private void FindChickenRendererIfNeeded()
    {
        if (chickenRenderer != null)
        {
            return;
        }

        ChickenController chicken = FindFirstObjectByType<ChickenController>();
        if (chicken != null)
        {
            chickenRenderer = chicken.GetComponent<SpriteRenderer>();
        }
    }

    private void CaptureChickenBaseColor()
    {
        if (chickenRenderer != null)
        {
            chickenBaseColor = chickenRenderer.color;
        }
    }

    private Color MultiplyColor(Color baseColor, Color tint)
    {
        return new Color(
            baseColor.r * tint.r,
            baseColor.g * tint.g,
            baseColor.b * tint.b,
            baseColor.a);
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