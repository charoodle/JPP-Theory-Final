using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trebuchet : ProjectileLauncher
{
    // TODO: Make these protected
    public GameObject projectile;
    public Transform projectileSpawnPoint;
    public Transform reloadHingeArmAttachPoint;
    public Rigidbody counterweightRb;
    public Rigidbody slingArmRb;
    public Rigidbody mainArmRb;
    public GameObject mainArm;
    public HingeJoint postHingeJoint;
    public LineRenderer rope;

    protected Coroutine shootingCoroutine;
    protected Coroutine tetherCoroutine;

    // Material set for the projectile after launch.
    [SerializeField] protected PhysicMaterial rockMaterial;

    protected bool isLaunching
    {
        get
        {
            return shootingCoroutine != null;
        }
    }

    protected override void Start()
    {
        Init(doneReloadPopupTime: 5f, ammoToRefillPerReload: 1);

        base.Start();

        // TEMP
        tetherCoroutine = StartCoroutine(TestRopeTetherToProjectile());
    }

    private IEnumerator TestRopeTetherToProjectile()
    {
        while(true)
        {
            rope.positionCount = 2;

            Vector3 slingPoint = reloadHingeArmAttachPoint.transform.position;
            rope.SetPosition(0, slingPoint);

            Vector3 projectilePoint = projectile.transform.position;
            rope.SetPosition(1, projectilePoint);

            yield return null;
        }
    }

    private IEnumerator TestRopeDetachFromProjectile()
    {
        if(tetherCoroutine != null)
            StopCoroutine(tetherCoroutine);

        rope.positionCount = 0;

        yield break;
    }

    protected override bool LaunchProjectile_Validation()
    {
        // Cannot launch again if already launching
        if (isLaunching)
            return false;

        return base.LaunchProjectile_Validation();
    }

    public override void LaunchProjectile()
    {
        if (!LaunchProjectile_Validation())
            return;

        shootingCoroutine = StartCoroutine(ShootTrebuchet());
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
                StartCoroutine(TestRopeDetachFromProjectile());
                ReleaseProjectileFromSling();
                shootingCoroutine = null;
                yield break;
            }

            yield return null;
        }
    }

    private void AttachRopeSlingToProjectile(Projectile projectile)
    {
        
    }

    private void DetachRopeSlingFromProjectile()
    {

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
        // Subtract ammo count
        ammoCount--;

        // Launch projectile
        HingeJoint projectileToArmHinge = projectile.GetComponent<HingeJoint>();
        Destroy(projectileToArmHinge);

        // Change material so projectile collides with ground like a rock
        Collider collider = projectile.GetComponent<Collider>();
        if(collider)
        {
            collider.material = rockMaterial;
        }

        // TODO: Add some slight left/right deviation
        // ...
    }

    protected override bool CheckForLaunchInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    protected override void LaunchProjectile_Forwards(Projectile projectile, float launchForce, ref bool targetFound, Vector3 crosshairTarget)
    {
        // Make the arm rotate to launch the projectile
        // Not used?

        throw new System.NotImplementedException();
    }

    protected override IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        yield return ReloadTrebuchet();
    }

    private IEnumerator ReloadTrebuchet()
    {
        // Freeze arm and weight from moving
        yield return FreezeArmAndWeightVelocities();

        // Reset main arm to neutral rotation
        yield return ResetEntireArmToNeutralRotation();

        // Load another projectile into it
        yield return LoadProjectile();
    }

    /// <summary>
    /// Stop the arm and weight from wiggling around.
    /// </summary>
    private IEnumerator FreezeArmAndWeightVelocities()
    {
        // Bring rigidbodies to zero velocity over a time duration
        float time = 0.0f;
        float duration = 5f;
        while (time < duration)
        {
            time += Time.deltaTime;

            // How much time has elapsed as a percent
            float pct = time / duration;

            // Reduce arm and weight velocities to 0
            yield return ReduceRigidbodyToZeroVelocity(counterweightRb, pct);
            yield return ReduceRigidbodyToZeroVelocity(mainArmRb, pct);

            yield return new WaitForFixedUpdate();
        }

        // Freeze rigidbodies
        mainArmRb.isKinematic = true;
        counterweightRb.isKinematic = true;
        yield break;
    }

    /// <summary>
    /// Make a rigidbody lerp towards zero velocity.
    /// </summary>
    /// <param name="lerpPct">Percent from 0 to 1.</param>
    /// <returns></returns>
    private IEnumerator ReduceRigidbodyToZeroVelocity(Rigidbody rb, float lerpPct)
    {
        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, lerpPct);
        yield break;
    }

    /// <summary>
    /// Rotate the trebuchet arm backwards until it reaches a local rotation angle of < 3 degrees.
    ///     Assumes the trebuchet at 0* (local rotation) is the neutral position (waiting to be shot).
    /// </summary>
    /// <param name="neutralXRotation"></param>
    /// <returns></returns>
    private IEnumerator ResetEntireArmToNeutralRotation()
    {
        // Make weight free so it doesn't look so weird when resetting arm
        counterweightRb.isKinematic = false;

        // Convert from local hinge joint anchor to point in world
        Vector3 hingeJointLocal = postHingeJoint.anchor;
        Vector3 hingeJointWorld = postHingeJoint.transform.TransformPoint(hingeJointLocal);
        Vector3 axis = Vector3.right;   //x axis
        float angleSpeed = -1f;  // rotate backwards

        // Rotate around the position for x seconds
        float origMainArmRotation = mainArm.transform.localRotation.eulerAngles.x;
        float targetMainArmXRotation = 3f;

        while (mainArm.transform.localRotation.eulerAngles.x > targetMainArmXRotation)
        {
            // How many pct is the rotation complete so far, from orig position to target position?
            //  - subtract by origMainArmRotation because it would otherwise offset the percentage calculation?
            //  - TODO: problem - euler angles have wonky values?
            //      - inspector eulers differs tfm.rotation.eulers call (quaternions?)
            float armPctRotation = (mainArm.transform.localRotation.eulerAngles.x - origMainArmRotation) / (targetMainArmXRotation - origMainArmRotation);

            // Make the weight go towards 0 velocity anyways, even with wonky percent numbers
            yield return ReduceRigidbodyToZeroVelocity(counterweightRb, armPctRotation);

            // Rotate arm around hinge post
            mainArm.transform.RotateAround(hingeJointWorld, axis, angleSpeed);

            yield return new WaitForFixedUpdate();
        }

        // Freeze weight to prepare for next shot
        counterweightRb.isKinematic = true;

        yield break;
    }

    /// <summary>
    /// Load and prepare a projectile to be shot again.
    /// </summary>
    private IEnumerator LoadProjectile()
    {
        // Can unfreeze main arm. Relies on unfreezing weight to fire.
        mainArmRb.isKinematic = false;
        mainArmRb.velocity = Vector3.zero;

        // Make sure weight is frozen
        counterweightRb.isKinematic = true;

        // Spawn another projectile at its spawn point
        Projectile projectile = SpawnProjectile(projectileSpawnPoint);

        // Move hinge joint anchor (local) of ball to reloadHingeArmAttachPoint's position (world)
        //  -- get world position as local position relative to the ball
        HingeJoint hingejoint = projectile.gameObject.GetComponent<HingeJoint>();
        Vector3 armAttachPointLocalToProjectile = projectile.transform.InverseTransformPoint(reloadHingeArmAttachPoint.transform.position);
        hingejoint.anchor = armAttachPointLocalToProjectile;

        // Connect hingejoint to the trebuchet's sling arm
        hingejoint.connectedBody = slingArmRb;

        // Assign projectile
        this.projectile = projectile.gameObject;

        // Attach rope to projectile
        tetherCoroutine = StartCoroutine(TestRopeTetherToProjectile());

        yield break;
    }
}
