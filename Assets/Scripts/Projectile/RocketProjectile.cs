using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : Projectile
{
    /// <summary>
    /// Spawned on collision with something.
    /// </summary>
    [SerializeField] GameObject rocketExplosionPrefab;
    [SerializeField] float _speed;
    float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            if (value <= 0)
                Debug.LogError($"Cannot have rocket projectile speed set to <= 0: value={value}");
            _speed = value;
        }
    }
    float gravityForce = 0.3f;
    protected bool alreadyExploded = false;


    private void FixedUpdate()
    {
        if (!enabled)
            return;

        // Make it go forward
        Vector3 forward = (Vector3.forward * speed * Time.deltaTime);
        if (forward.magnitude > 0)
            transform.Translate(forward);

        // Make it go down; less force than gravity
        Vector3 down = (Vector3.down * gravityForce * Time.deltaTime);
        if (down.magnitude > 0)
            transform.Translate(down);

        // Rocket points towards direction of velocity
        Vector3 velocity = forward + down;
        if(velocity.magnitude > 0)
            transform.forward = transform.TransformDirection(velocity);

        // TODO: Make it go slow, lock onto target, then speed up and follow it
        //  TODO: Change direction?
        //transform.TransformDirection(Vector3.forward * speed * Time.deltaTime);
    }

    public void InitializeRocket(float launchForce)
    {
        speed = launchForce;
    }

    protected override void DestroyInAFewSeconds()
    {
        destroyAfterSeconds = 10f;
        base.DestroyInAFewSeconds();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // Not an enemy
        if(!Utils.IsEnemy(collision.gameObject))
        {
            // Spawn explosion prefab, with up direction facing collision's normal direction (if its not an enemy)
            ContactPoint point = collision.GetContact(0);
            CreateExplosion(point.point, point.normal);
        }
        else
        {
            // Spawn explosion prefab, with up direction facing world up
            ContactPoint point = collision.GetContact(0);
            CreateExplosion(point.point, Vector3.up);
        }
    }

    protected void CreateExplosion(Vector3 position, Vector3 up)
    {
        // Only explode once
        if (alreadyExploded)
            return;

        alreadyExploded = true;
        GameObject explosion = Instantiate(rocketExplosionPrefab, position, rocketExplosionPrefab.transform.rotation);
        explosion.transform.up = up;
    }

    protected void DisableRocket()
    {
        // Stop it from moving
        this.enabled = false;

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

    protected override void DestroyProjectile()
    {
        // After explosion, wait until particle system lifetime and then destroy object
        StartCoroutine(LetRocketTrailParticlesFadeBeforeDestroy());
    }

    protected IEnumerator LetRocketTrailParticlesFadeBeforeDestroy()
    {
        // Disable the rocket
        DisableRocket();

        // Wait for seconds
        yield return new WaitForSeconds(5f);

        base.DestroyProjectile();

        yield break;
    }

    
}
