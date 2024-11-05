using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetProjectile : Projectile
{
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
}
