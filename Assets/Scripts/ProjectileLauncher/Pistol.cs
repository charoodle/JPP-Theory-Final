using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : ProjectileLauncher
{
    protected override void Start()
    {
        // Init - Shorter popup time after reload finished
        Init(doneReloadPopupTime: 0.5f, ammoToRefillPerReload: 15);
        base.Start();
    }

    protected override bool CheckForLaunchInput()
    {
        // Use mouse click to desire shooting
        return Input.GetMouseButtonDown(0);
    }

    protected override IEnumerator ReloadProjectileCoroutine_RefillAmmunitionCount(int ammoToRefill)
    {
        this.ammoCount = ammoToRefill;
        yield break;
    }

    protected override void LaunchProjectile_Forwards(Projectile projectile, float launchForce, ref bool targetFound, Vector3 crosshairTarget)
    {
        base.LaunchProjectile_Forwards(projectile, launchForce, ref targetFound, crosshairTarget);

        // Use rigidbody of pistol bullet to launch it forwards
        Rigidbody bulletRb = projectile.GetComponent<Rigidbody>();
        if (!bulletRb)
        {
            Debug.LogError("No projectile rigidbody found.");
            return;
        }
        Vector3 forward = projectile.transform.forward * launchForce;
        bulletRb.AddForce(forward, ForceMode.Impulse);
    }
}
