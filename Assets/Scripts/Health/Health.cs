using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] protected float startingHealth = 3f;

    [SerializeField] protected float _health;
    protected float health
    {
        get { return _health; }
        set
        {
            // Health cannot go into negatives
            if (value <= 0)
                value = 0;

            _health = value;
        }
    }

    protected virtual void Start()
    {
        health = startingHealth;
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
}
