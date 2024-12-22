using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public delegate void HealthChangedAction(float health);
    public event HealthChangedAction OnHealthSet;
    public event HealthChangedAction OnDie;

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

#if UNITY_EDITOR
    [Header("Debug Options")]
    [SerializeField] protected bool debugGodMode = false;
    [SerializeField] protected bool debugDie = false;

    private void OnValidate()
    {
        if (debugDie && Application.isPlaying)
        {
            debugDie = false;
            TakeDamage(health);
        }
    }
#endif

    protected virtual void Start()
    {
        RefillHealthToFull();
    }

    public virtual void TakeDamage(float value)
    {
#if UNITY_EDITOR
        // Debug - don't take damage if inspector says so
        if (debugGodMode)
            return;
#endif

        health -= value;
        if (health <= 0)
            Die();
    }

    protected virtual void Die()
    {
        // Tell listeners this object has died; dummy health value
        OnDie?.Invoke(health);
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
