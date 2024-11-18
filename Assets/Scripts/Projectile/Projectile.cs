using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    public bool allowCollisions = true;

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

    /// <summary>
    /// Assumes only 1 particle system attached to projectile as child.
    /// </summary>
    protected float particleSystemLifetime = 0f;

    protected virtual void Start()
    {
        DestroyInAFewSeconds();

        // Assumes particle system has constant lifetime
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        if(ps)
            particleSystemLifetime = ps.main.startLifetime.constant;
    }

    protected virtual void DestroyInAFewSeconds()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (!allowCollisions)
            return;

        TakeHealthAwayFrom(collision);

        DestroyProjectile();
    }

    protected virtual bool TakeHealthAwayFrom(Collision collision)
    {
        // Take health away from object when hit with this projectile
        //  Colliders usually on child objects. Scripts on parent objects.
        Health health = collision.gameObject.GetComponentInParent<Health>();
        if (health)
        {
            health.TakeDamage(damage);
            return true;
        }
        return false;
    }

    protected virtual void DestroyProjectile()
    {
        // Disable collisions once marked for destruction
        allowCollisions = false;

        // Wait for the particle trails to finish their lifetime before destroying (if it has any).
        StartCoroutine(LetParticleTrailFadeBeforeDestroy());
    }

    protected virtual IEnumerator LetParticleTrailFadeBeforeDestroy()
    {
        // If there are particles, disable the projectile colliders and renderers (except particles)
        if(particleSystemLifetime > 0)
        {
            DisableProjectileCollidersAndRenderers_ExceptParticles();

            // Wait for particle system lifetime to finish
            yield return new WaitForSeconds(particleSystemLifetime);
        }
        
        // Destroy the particles
        Destroy(gameObject);

        yield break;
    }

    protected virtual void DisableProjectileCollidersAndRenderers_ExceptParticles()
    {
        // Stop it from moving
        StopProjectileMovement();

        // Disable any colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
            col.enabled = false;

        // Disable any renderers (except particle system's)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            if (rend as ParticleSystemRenderer)
            {
                // Disable particle system from emitting
                ParticleSystem ps = rend.GetComponent<ParticleSystem>();
                ps.Stop();

                // Do not disable the particle system renderer; let the rocket trail render still
                continue;
            }

            rend.enabled = false;
        }
    }

    protected abstract void StopProjectileMovement();
}
