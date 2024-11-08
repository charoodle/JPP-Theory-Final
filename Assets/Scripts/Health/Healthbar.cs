using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour
{
    protected Health objectHealth;
    protected Slider slider;
    [SerializeField] protected TextMeshProUGUI healthText;

    protected void OnEnable()
    {
        slider = GetComponent<Slider>();
        
        if (!objectHealth)
        {
            objectHealth = GetComponentInParent<Health>();
            if (!objectHealth)
                Debug.LogWarning("No health component found.");
        }

        if(objectHealth)
            objectHealth.OnHealthSet += SetHealthbarValue;

        // Initialize
        SetHealthbarValue(objectHealth.health);
    }

    protected void OnDisable()
    {
        if(objectHealth)
            objectHealth.OnHealthSet -= SetHealthbarValue;
    }

    protected void SetHealthbarValue(float health)
    {
        // Find health of object as a percent
        float pct = health / objectHealth.startingHealth;

        // Clamp between 0 and 1
        pct = Mathf.Clamp(pct, 0f, 1f);

        // Update healthbar
        slider.value = pct;

        // Update healthbar text
        healthText.SetText($"{health} / {objectHealth.startingHealth}");
    }
}
