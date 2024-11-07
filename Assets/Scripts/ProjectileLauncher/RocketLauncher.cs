using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : ProjectileLauncher
{
    protected override void Start()
    {
        // Init - Shorter popup time after reload finished
        Init(ammoToRefillPerReload: 1);

        base.Start();
    }

    protected override bool CheckForLaunchInput()
    {
        // Must use left and right mouse button to fire
        return Input.GetMouseButton(0) && Input.GetMouseButton(1);
    }

    protected override void LaunchProjectile_Forwards(Projectile projectile, float launchForce)
    {
        // Tell rocket how fast it should go
        RocketProjectile rocket = projectile as RocketProjectile;
        if (!rocket)
            Debug.LogError("No RocketProjectile was found on given projectile prefab", projectile.gameObject);

        // Let rocket propel itself
        rocket.InitializeRocket(launchForce);
        return;
    }

    protected override IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        // Loading new rocket in

        // Calibrating targeting systems

        // Preparing warhead

        // OK

        yield return base.ReloadProjectileCoroutine_WaitForSeconds();
    }
}
