using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TimeOfDayLighting : MonoBehaviour
{
    [Header("Clock")]
    [SerializeField] private HudClock hudClock;

    [Header("Controlled Lights")]
    [SerializeField] private Light2D[] controlledLights;

    [Header("Universal Light")]
    [SerializeField] private Light2D universalLight;
    [SerializeField, Min(0f)] private float universalSunriseIntensity = 0.4f;

    [Header("Sunrise")]
    [SerializeField] private Color sunriseColor = new Color(1f, 0.72f, 0.36f, 1f);
    [SerializeField, Min(0f)] private float globalSunriseIntensity = 0.7f;
    [SerializeField, Min(0f)] private float localSunriseIntensityMultiplier = 1.2f;

    [Header("Dawn Timing")]
    [SerializeField, Range(0, 23)] private int fasterChangeStartHour = 4;
    [SerializeField, Range(0, 59)] private int fasterChangeStartMinute;
    [SerializeField, Range(0f, 1f)] private float lightProgressAtFasterChangeStart = 0.35f;

    private Color[] nightColors;
    private float[] nightIntensities;
    private float universalNightIntensity;

    private void Awake()
    {
        if (hudClock == null)
        {
            hudClock = FindFirstObjectByType<HudClock>();
        }

        FindUniversalLightIfNeeded();
    }

    private void Start()
    {
        CaptureNightLighting();
        ApplyLighting();
    }

    private void Update()
    {
        ApplyLighting();
    }

    private void OnValidate()
    {
        universalSunriseIntensity = Mathf.Max(0f, universalSunriseIntensity);
        globalSunriseIntensity = Mathf.Max(0f, globalSunriseIntensity);
        localSunriseIntensityMultiplier = Mathf.Max(0f, localSunriseIntensityMultiplier);
        fasterChangeStartHour = Mathf.Clamp(fasterChangeStartHour, 0, 23);
        fasterChangeStartMinute = Mathf.Clamp(fasterChangeStartMinute, 0, 59);
        lightProgressAtFasterChangeStart = Mathf.Clamp01(lightProgressAtFasterChangeStart);

        if (sunriseColor.a <= 0f)
        {
            sunriseColor.a = 1f;
        }
    }

    private void CaptureNightLighting()
    {
        int lightCount = controlledLights == null ? 0 : controlledLights.Length;
        nightColors = new Color[lightCount];
        nightIntensities = new float[lightCount];

        for (int i = 0; i < lightCount; i++)
        {
            Light2D lightToControl = controlledLights[i];
            if (lightToControl == null)
            {
                continue;
            }

            nightColors[i] = lightToControl.color;
            nightIntensities[i] = lightToControl.intensity;
        }

        FindUniversalLightIfNeeded();
        if (universalLight != null)
        {
            universalNightIntensity = universalLight.intensity;
        }
    }

    private void ApplyLighting()
    {
        if (hudClock == null)
        {
            return;
        }

        float progress = GetLightingProgress(hudClock.Progress01);
        ApplyControlledLights(progress);
        ApplyUniversalLight();
    }

    private void ApplyControlledLights(float progress)
    {
        if (controlledLights == null || nightColors == null || nightIntensities == null)
        {
            return;
        }

        for (int i = 0; i < controlledLights.Length; i++)
        {
            Light2D lightToControl = controlledLights[i];
            if (lightToControl == null || i >= nightColors.Length || i >= nightIntensities.Length)
            {
                continue;
            }

            lightToControl.color = Color.Lerp(nightColors[i], sunriseColor, progress);
            lightToControl.intensity = Mathf.Lerp(nightIntensities[i], GetSunriseIntensity(lightToControl, nightIntensities[i]), progress);
        }
    }

    private void ApplyUniversalLight()
    {
        FindUniversalLightIfNeeded();
        if (universalLight == null)
        {
            return;
        }

        float progress = GetUniversalLightProgress(hudClock.Progress01);
        universalLight.intensity = Mathf.Lerp(universalNightIntensity, universalSunriseIntensity, progress);
    }

    private float GetUniversalLightProgress(float clockProgress)
    {
        float clampedProgress = Mathf.Clamp01(clockProgress);
        float fasterChangeStartProgress = Mathf.Clamp(hudClock.GetProgressAtTime(fasterChangeStartHour, fasterChangeStartMinute), 0.01f, 0.99f);

        if (clampedProgress <= fasterChangeStartProgress)
        {
            return 0f;
        }

        float dawnProgress = (clampedProgress - fasterChangeStartProgress) / (1f - fasterChangeStartProgress);
        return Mathf.SmoothStep(0f, 1f, dawnProgress);
    }

    private float GetLightingProgress(float clockProgress)
    {
        float clampedProgress = Mathf.Clamp01(clockProgress);
        float fasterChangeStartProgress = Mathf.Clamp(hudClock.GetProgressAtTime(fasterChangeStartHour, fasterChangeStartMinute), 0.01f, 0.99f);
        float lightProgressAtStart = Mathf.Clamp(lightProgressAtFasterChangeStart, 0.01f, 0.99f);

        if (clampedProgress <= fasterChangeStartProgress)
        {
            float earlyProgress = clampedProgress / fasterChangeStartProgress;
            return Mathf.Lerp(0f, lightProgressAtStart, Mathf.SmoothStep(0f, 1f, earlyProgress));
        }

        float dawnProgress = (clampedProgress - fasterChangeStartProgress) / (1f - fasterChangeStartProgress);
        return Mathf.Lerp(lightProgressAtStart, 1f, Mathf.SmoothStep(0f, 1f, dawnProgress));
    }

    private float GetSunriseIntensity(Light2D lightToControl, float nightIntensity)
    {
        if (lightToControl.lightType == Light2D.LightType.Global)
        {
            return globalSunriseIntensity;
        }

        return nightIntensity * localSunriseIntensityMultiplier;
    }

    private void FindUniversalLightIfNeeded()
    {
        if (universalLight != null)
        {
            return;
        }

        Light2D[] lights = FindObjectsOfType<Light2D>();
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].name == "UniversalLight")
            {
                universalLight = lights[i];
                return;
            }
        }
    }
}