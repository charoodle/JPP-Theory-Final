using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetCounterweightAdjustment : Interactable
{
    [SerializeField] TrebuchetCounterweight weight;

    [Range(-50f, 50f)]
    [SerializeField] protected float adjustWeightBy;

    private string originalInteractText;

    protected override void Start()
    {
        base.Start();
        originalInteractText = interactText;

        OnWeightValueChanged(weight.mass);
    }

    public override void InteractWith()
    {
        weight.mass += adjustWeightBy;
    }

    private void OnEnable()
    {
        if(weight)
            weight.OnWeightValueChanged += OnWeightValueChanged;
    }

    private void OnDisable()
    {
        if(weight)
            weight.OnWeightValueChanged -= OnWeightValueChanged;
    }

    protected void OnWeightValueChanged(float newWeight)
    {
        // If new weight + current weight goes out of min/max trebuchet weight limits, show on the interactable text
        if (newWeight + adjustWeightBy > weight.maxWeight)
            interactText = originalInteractText + "\nWeight: " + weight.mass +"kg (Max)";
        // Weight value can be negative
        else if (newWeight + adjustWeightBy < weight.minWeight)
            interactText = originalInteractText + "\nWeight: " + weight.mass + "kg (Min)";
        else
            interactText = originalInteractText + "\nWeight: " + weight.mass + "kg";
    }
}
