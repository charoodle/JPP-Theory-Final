using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TrebuchetCounterweight : MonoBehaviour
{
    public delegate void WeightChangedAction(float newWeight);
    public event WeightChangedAction OnWeightValueChanged;

    /// <summary>
    /// In kilograms.
    /// </summary>
    public float mass
    {
        get
        {
            return rb.mass;
        }
        set
        {
            // Clamp weight between value
            SetWeight(value);
        }
    }
    /// <summary>
    /// Minimum weight the counterweight can be.
    /// </summary>
    public float minWeight { get { return _minWeight; } }
    /// <summary>
    /// Max weight the counterweight can be.
    /// </summary>
    public float maxWeight { get { return _maxWeight; } }

    [SerializeField] protected float _minWeight;
    [SerializeField] protected float _maxWeight;

    protected Rigidbody rb;
    //protected float _mass;



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected void SetWeight(float weight)
    {
        // Clamp weight between min and max
        weight = Mathf.Clamp(weight, minWeight, maxWeight);

        // No decimals
        weight = Mathf.RoundToInt(weight);
        
        rb.mass = weight;

        // Send event to anyone listening
        OnWeightValueChanged?.Invoke(weight);
    }
}
