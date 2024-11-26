using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetProjectile : Projectile
{
    [SerializeField] ParticleSystem particles;
    [SerializeField] ParticleSystem collision_dirtParticles;
    Rigidbody rb;

    protected override void Start()
    {
        rb = GetComponent<Rigidbody>();
        base.Start();
    }

    protected override void DestroyInAFewSeconds()
    {
        // Do not destroy in a few seconds. Wait for it to be launched instead.
    }

    public void EnableParticles()
    {
        particles.Play();
    }

    public void DisableParticles()
    {
        particles.Stop();
    }

    protected void OnEnable()
    {
        // Don't allow projectile to be destroyed when its being reloaded
        allowCollisions = false;
    }

    protected void Update()
    {
        // Destroy if projectile infinite falls into void.
        if (transform.position.y < -30f)
            DestroyProjectile();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (!allowCollisions)
            return;

        // If touch anything, disable flying particles
        DisableParticles();

        // Play collision particles
        if (collision_dirtParticles)
        {
            Instantiate(collision_dirtParticles, transform.position, collision_dirtParticles.transform.rotation);
        }

        // Stop moving. Embed ball within collision point.
        StopProjectileMovement();
        transform.position = collision.GetContact(0).point;
        Debug.Log("Embedding self into contact point.");

        // If hit something with health, then take health away from it.
        if (TakeHealthAwayFrom(collision))
        {
            //DestroyProjectile();
        }

        // Disable any other collisions so it cannot take health away from anything else.
        allowCollisions = false;
    }

    protected override void StopProjectileMovement()
    {
        this.rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
