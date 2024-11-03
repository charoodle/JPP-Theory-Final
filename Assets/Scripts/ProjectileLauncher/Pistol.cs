using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : ProjectileLauncher
{
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected int ammoCount;

    public void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            LaunchProjectile();
        }
    }

    public override void LaunchProjectile()
    {
        // Out of ammo
        if (ammoCount <= 0)
        {
            // Start reload instead
            ReloadProjectile();
            return;
        }

        // Is currently reloading
        if(isReloading)
        {
            return;
        }

        // Subtract 1 ammo
        ammoCount--;

        // Spawn projectile at barrel, facing same forward dir as barrel
        GameObject bullet = SpawnProjectile(shootPoint);

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

    protected override IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        yield return new WaitForSeconds(3f);
    }

    protected override IEnumerator ReloadProjectileCoroutine_RefillAmmunitionCount()
    {
        int reloadToAmmo = 7;
        ammoCount = reloadToAmmo;
        yield break;
    }
}
