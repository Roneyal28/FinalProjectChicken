using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthBarSlider;

    [Header("Trail")]
    [SerializeField] private float trailDelay = 0.25f;
    [SerializeField] private float trailLerpSpeed = 6f;

    private float targetHealth;
    private float trailDelayTimer;

    public bool HasBothSliders => healthSlider != null && easeHealthBarSlider != null;
    
    public void SetMaxHealth(int maxHealth)
    {
        SetSliderMax(healthSlider, maxHealth);
        SetSliderMax(easeHealthBarSlider, maxHealth);
        targetHealth = maxHealth;
        trailDelayTimer = 0f;
    }

    public void SetHealth(int health)
    {
        targetHealth = health;
        trailDelayTimer = trailDelay;

        if (healthSlider != null)
        {
            healthSlider.value = health;
        }

        if (easeHealthBarSlider != null && easeHealthBarSlider.value < health)
        {
            easeHealthBarSlider.value = health;
        }
    }

    void Update()
    {
        if (easeHealthBarSlider == null)
        {
            return;
        }

        if (trailDelayTimer > 0f)
        {
            trailDelayTimer -= Time.deltaTime;
            return;
        }

        easeHealthBarSlider.value = Mathf.Lerp(
            easeHealthBarSlider.value,
            targetHealth,
            trailLerpSpeed * Time.deltaTime
        );

        if (Mathf.Abs(easeHealthBarSlider.value - targetHealth) < 0.01f)
        {
            easeHealthBarSlider.value = targetHealth;
        }
    }

    private void SetSliderMax(Slider slider, int maxHealth)
    {
        if (slider == null)
        {
            return;
        }

        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }
}
