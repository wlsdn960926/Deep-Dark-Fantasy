using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPSlider : MonoBehaviour
{
    public HealthSystem bossHealthSystem; // Reference to the boss's HealthSystem component
    public Slider slider; // Reference to the slider UI element

	private void Awake()
	{        
        slider = GameObject.Find("Slider").GetComponent<Slider>();
	}

	private void Start()
    {
        // Ensure that both references are set
        if (bossHealthSystem == null || slider == null)
        {
            Debug.LogError("BossHPSlider is missing references to bossHealthSystem or slider!");
            return;
        }

        // Set the maximum value of the slider to the boss's maximum health
        slider.maxValue = bossHealthSystem.MaxHealth;

        // Update the slider value to reflect the boss's current health
        UpdateSliderValue();
    }

    private void UpdateSliderValue()
    {
        // Set the slider value to the boss's current health
        slider.value = bossHealthSystem.CurrentHealth;
    }

    private void OnEnable()
    {
        // Subscribe to the boss's health change events
        bossHealthSystem.OnDamage += UpdateSliderValue;
        bossHealthSystem.OnHeal += UpdateSliderValue;
    }

    private void OnDisable()
    {
        // Unsubscribe from the boss's health change events
        bossHealthSystem.OnDamage -= UpdateSliderValue;
        bossHealthSystem.OnHeal -= UpdateSliderValue;
    }
}
