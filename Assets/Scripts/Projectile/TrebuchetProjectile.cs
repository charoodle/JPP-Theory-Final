using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetProjectile : Projectile
{
    [SerializeField] ParticleSystem particles;
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

        // If touch ground, disable particles
        if (collision.gameObject.CompareTag("Ground"))
            DisableParticles();

        // Trebuchet projectile can roll and hit the castle
        if (TakeHealthAwayFrom(collision))
        {
            DestroyProjectile();
        }
    }

    protected override void StopProjectileMovement()
    {
        this.rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
