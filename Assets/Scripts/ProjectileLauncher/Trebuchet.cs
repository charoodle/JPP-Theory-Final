using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trebuchet : ProjectileLauncher
{
    protected override bool CheckForLaunchInput()
    {
        return Input.GetMouseButtonDown(0);
    }

    protected override void LaunchProjectile_Forwards(Projectile projectile, float launchForce)
    {
        // Make the arm rotate to launch the projectile

        throw new System.NotImplementedException();
    }

    protected override IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        throw new System.NotImplementedException();
    }
}
