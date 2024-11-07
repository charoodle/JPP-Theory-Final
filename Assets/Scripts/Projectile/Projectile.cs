using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [SerializeField] protected float _damage = 1f;

    /// <summary>
    /// How much damage it should do to things it hits.
    /// </summary>
    [SerializeField] protected float damage
    {
        get { return _damage; }
        set
        {
            // Damage cannot be < 0 for a projectile
            if(value < 0)
            {
                Debug.LogWarning("Cannot set damage to < 0.");
                value = 0;
            }
            damage = value;
        }
    }

    [SerializeField] protected float _destroyAfterSeconds = 5f;
    [SerializeField] protected float destroyAfterSeconds
    {
        get
        {
            return _destroyAfterSeconds;
        }
        set
        {
            if (value <= 0)
                Debug.LogError($"Projectile cannot have destroyedAfterSeconds with value <= 0. value={value}");
            _destroyAfterSeconds = value;
        }
    }

    protected virtual void Start()
    {
        DestroyInAFewSeconds();
    }

    protected virtual void DestroyInAFewSeconds()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Take health away from object when hit with this projectile
        //  Colliders usually on child objects. Scripts on parent objects.
        Health health = collision.gameObject.GetComponentInParent<Health>();
        if(health)
        {
            health.TakeDamage(damage);
            DestroyProjectile();
        }

    }

    protected virtual void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
