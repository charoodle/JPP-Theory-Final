using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TrebuchetCounterweightUI : MonoBehaviour
{
    [SerializeField] TrebuchetCounterweight weight;
    [SerializeField] TextMeshProUGUI weightText;

    private void Start()
    {
        // Update text to reflect current mass
        OnWeightValueChanged(weight.mass);
    }

    private void OnEnable()
    {
        if (weight)
            weight.OnWeightValueChanged += OnWeightValueChanged;
    }

    private void OnDisable()
    {
        if (weight)
            weight.OnWeightValueChanged -= OnWeightValueChanged;
    }

    private void OnWeightValueChanged(float newWeight)
    {
        weightText.text = $"{newWeight}kg";
    }
}
