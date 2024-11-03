using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : ProjectileLauncher
{
    protected override void Start()
    {
        // Shorter popup time after reload finished
        doneReloadPopupTime = 0.5f;
        ammoToRefillPerReload = 7;

        base.Start();
    }

    protected override bool CheckForLaunchInput()
    {
        // Use mouse click to desire shooting
        return Input.GetMouseButtonDown(0);
    }

    protected override IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        yield return new WaitForSeconds(3f);
    }

    protected override IEnumerator ReloadProjectileCoroutine_RefillAmmunitionCount(int ammoToRefill)
    {
        this.ammoCount = ammoToRefill;
        yield break;
    }

    protected override void LaunchProjectile_Forwards(Projectile projectile, float launchForce)
    {
        // Use rigidbody of pistol bullet to launch it forwards
        Rigidbody bulletRb = projectile.GetComponent<Rigidbody>();
        if (!bulletRb)
        {
            Debug.LogError("No projectile rigidbody found.");
            return;
        }
        Vector3 forward = Vector3.forward * launchForce;
        bulletRb.AddRelativeForce(forward, ForceMode.Impulse);
    }
}
