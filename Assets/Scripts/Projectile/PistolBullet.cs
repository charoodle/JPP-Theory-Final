using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PistolBullet : Projectile
{
    // Doesn't really need to do anything except go forwards and be affected by gravity. Rigidbody covers this.

    Rigidbody rb;

    protected override void Start()
    {
        rb = GetComponent<Rigidbody>();
        base.Start();
    }

    protected override void StopProjectileMovement()
    {
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
