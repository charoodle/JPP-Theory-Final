using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public delegate void HealthChangedAction(float health);
    public event HealthChangedAction OnHealthSet;

    [SerializeField] protected float _startingHealth = 3f;
    [SerializeField] public float startingHealth
    {
        get { return _startingHealth; }
        protected set
        {
            _startingHealth = value;
        }
    }

    [SerializeField] protected float _health;
    public float health
    {
        get { return _health; }
        protected set
        {
            // Health cannot go into negatives
            if (value <= 0)
                value = 0;

            _health = value;

            if (OnHealthSet != null)
                OnHealthSet(_health);
        }
    }

    protected virtual void Start()
    {
        RefillHealthToFull();
    }

    public virtual void TakeDamage(float value)
    {
        health -= value;
        if (health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        DestroyObject();
    }

    protected virtual void DestroyObject()
    {
        Destroy(gameObject);
    }

    protected void RefillHealthToFull()
    {
        health = startingHealth;
    }
}
