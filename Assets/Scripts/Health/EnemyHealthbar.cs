using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthbar : Healthbar
{
    Canvas canvas;

    protected override void OnEnable()
    {
        canvas = GetComponentInParent<Canvas>();

        base.OnEnable();
    }

    protected override void SetHealthbarValue(float health)
    {
        base.SetHealthbarValue(health);

        // Show healthbar if not at full health. Full health = hidden.
        bool isFullHealth = health / objectHealth.startingHealth == 1f;
        ShowHealthbar(!isFullHealth);
    }

    protected void ShowHealthbar(bool value)
    {
        canvas.enabled = value;
    }
}
