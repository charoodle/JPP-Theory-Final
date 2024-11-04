using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetProjectile : Projectile
{
    protected override void Start()
    {
        // Destroy after 10 minutes.
        destroyAfterSeconds = 600f;
    }

    protected void Update()
    {
        // Destroy if projectile infinite falls into void.
        if (transform.position.y < -30f)
            Destroy(gameObject);
    }
}
