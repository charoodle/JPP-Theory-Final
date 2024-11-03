using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : ProjectileLauncher
{
    [SerializeField] protected Transform ShootPoint;

    public void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            LaunchProjectile();
        }
    }

    public override void LaunchProjectile()
    {
        // Spawn projectile at barrel, facing same forward dir as barrel
        GameObject projectile = SpawnProjectile(ShootPoint);

        // TODO: Launch projectile rigidbody forward with a very fast force
        // ...
    }

    public override void ReloadProjectile()
    {
        throw new System.NotImplementedException();
    }
}
