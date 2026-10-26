using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AgentHealthBar : MonoBehaviour
{
    [SerializeField] private StatScript stats;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        if (stats == null)
            stats = GetComponentInParent<StatScript>();
    }

    private void Update()
    {
        if (stats == null || healthSlider == null)
            return;

        int currentHealth = Mathf.RoundToInt(stats.GetHealth());
        int maxHealth = Mathf.RoundToInt(stats.GetMaxHealth());

        if (maxHealth <= 0)
            return;

        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthText != null)
            healthText.text = $"{currentHealth} / {maxHealth} HP";
    }
}
