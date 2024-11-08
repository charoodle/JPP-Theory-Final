using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetProjectile : Projectile
{
    protected void OnEnable()
    {
        // Don't allow projectile to be destroyed when its being reloaded
        allowCollisions = false;
    }

    protected override void Start()
    {
        // Don't destroy the projectile after seconds.
    }

    protected void Update()
    {
        // Destroy if projectile infinite falls into void.
        if (transform.position.y < -30f)
            Destroy(gameObject);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (!allowCollisions)
            return;

        // Trebuchet projectile can roll and hit the castle
        if (TakeHealthAwayFrom(collision))
            DestroyProjectile();
    }
}
