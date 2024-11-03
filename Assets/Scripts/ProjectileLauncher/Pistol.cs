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
        GameObject bullet = SpawnProjectile(ShootPoint);

        // Launch projectile rigidbody forward with a very fast force
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (!bulletRb)
        {
            Debug.LogError("No projectile rigidbody found.");
            return;
        }
        Vector3 forward = Vector3.forward * launchForce;
        bulletRb.AddRelativeForce(forward, ForceMode.Impulse);
    }

    public override void ReloadProjectile()
    {
        throw new System.NotImplementedException();
    }
}
