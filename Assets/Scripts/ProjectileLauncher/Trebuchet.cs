using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trebuchet : ProjectileLauncher
{

    public GameObject projectile;


    public Rigidbody counterweightRb;


    public GameObject mainArm;

    protected override void Start()
    {
        GunUICanvasEnabled(false);
        
        // TEMP
        ammoCount = 1;
    }

    public override void LaunchProjectile()
    {
        if (!LaunchProjectile_Validation())
            return;

        ammoCount--;

        StartCoroutine(ShootTrebuchet());
    }

    private IEnumerator ShootTrebuchet()
    {
        ReleaseCounterweight();

        /// Wait for ball to be about 90* (or slightly less) to launch
        bool readyToRelease = false;
        while (true)
        {
            float dot = GetProjectileDotProduct(projectile.transform, mainArm.transform);
            if (IsProjectileReadyToRelease(dot) && !readyToRelease)
            {
                readyToRelease = true;
            }

            // Release ball before highest point automatically. Break out of infinite loop.
            //  - dot product is at least at .95, so it should release before dot product hits 1 (90*)
            if (readyToRelease && IsProjectileReadyToReleaseFromSling(dot))
            {
                ReleaseProjectileFromSling();
                yield break;
            }

            yield return null;
        }
    }

    private void ReleaseCounterweight()
    {
        // Release counterweight to start launch
        counterweightRb.isKinematic = false;
    }

    private float GetProjectileDotProduct(Transform projectile, Transform mainArmCenter)
    {
        // Check for when point from ball to arm is near Vector3.up
        Vector3 worldUp = Vector3.up;
        Vector3 armToBall = (projectile.transform.position - mainArmCenter.transform.position).normalized;

        Debug.DrawRay(mainArmCenter.transform.position, armToBall * 3f, Color.red);
        Debug.DrawRay(mainArmCenter.transform.position, worldUp * 3f, Color.green);

        return Vector3.Dot(worldUp, armToBall);
    }

    private bool IsProjectileReadyToRelease(float dotProductOfProjectileToWorldUp)
    {
        // dot < 0 when neutral position to launch
        // dot == 0 when orthogonal
        // dot == 1 when near launch (same as Vector3.up)
        //  dot < 1 when past launch, have to be careful

        // Ball is near highest point; get ready to release
        return dotProductOfProjectileToWorldUp >= 0.95f;
    }

    private bool IsProjectileReadyToReleaseFromSling(float dotProductOfProjectileToWorldUp, float maxRandomDotProductOffset = 0.04f)
    {
        // Calculate a random point after 1 to release for some randomness
        float dotOffset = Random.Range(0, maxRandomDotProductOffset);
        return dotProductOfProjectileToWorldUp < 1 - dotOffset;
    }

    /// <summary>
    /// Detach ball from the sling of the arm.
    /// </summary>
    private void ReleaseProjectileFromSling()
    {
        // Launch projectile
        HingeJoint projectileToArmHinge = projectile.GetComponent<HingeJoint>();
        Destroy(projectileToArmHinge);

        // Add some slight left/right deviation
        // ...
    }


    // ----


    protected override bool CheckForLaunchInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    protected override void LaunchProjectile_Forwards(Projectile projectile, float launchForce)
    {
        // Make the arm rotate to launch the projectile
        // Not used?

        throw new System.NotImplementedException();
    }

    protected override IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        throw new System.NotImplementedException();
    }

    
}
