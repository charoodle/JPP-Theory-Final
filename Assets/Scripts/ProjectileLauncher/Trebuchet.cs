using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trebuchet : ProjectileLauncher
{
    // TODO: Make these protected
    public GameObject trebuchetLaunchPreventer;     // holds the long arm down to prevent it from launching
    public GameObject trebuchetReloader;     // holds the long arm down to prevent it from launching
    public GameObject projectile;
    public Transform projectileSpawnPoint;
    public Transform reloadHingeArmAttachPoint;
    public Rigidbody counterweightRb;
    public Rigidbody slingArmRb;
    public Rigidbody mainArmRb;
    public GameObject mainArm;
    public HingeJoint postHingeJoint;
    public HingeJoint counterweightHingeJoint;
    public LineRenderer rope;

    protected Coroutine shootingCoroutine;
    protected Coroutine tetherCoroutine;

    /// <summary>
    /// Counterweight's position at rest/start (local).
    /// </summary>
    protected Vector3 counterWeightLocalRestingPosition;

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

        // Get counterweight's starting position (local)
        counterWeightLocalRestingPosition = counterweightRb.transform.localPosition;

        StartCoroutine(Start_LoadProjectile());
    }

    /// <summary>
    /// Load an initial projectile to start with.
    /// </summary>
    /// <returns></returns>
    private IEnumerator Start_LoadProjectile()
    {
        // Load another projectile into it
        yield return LoadProjectile();

        // Hold down the long arm end from launching the projectile
        yield return HoldDownArmFromLaunching();
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
        {
            StopCoroutine(tetherCoroutine);
        }

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
        float time = 0f;
        float maxTimeUntilForceRelease = 5f;
        while (time < maxTimeUntilForceRelease)
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
                yield return DetachBall();
                yield break;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // If hasn't reached max height, probably bad launch from something getting in way of ball. Force detach.
        yield return DetachBall();
        yield break;

        IEnumerator DetachBall()
        {
            // Camera shake
            CameraShaker.instance.Shake(onLaunch_ShakeSeconds, onLaunch_ShakeIntensity);

            // Send event that projectile has launched.
            OnProjectileLaunch?.Invoke();

            StartCoroutine(TestRopeDetachFromProjectile());
            ReleaseProjectileFromSling();
            shootingCoroutine = null;
            // Start particle system of projectile
            TrebuchetProjectile trebuchetProj = projectile.GetComponent<TrebuchetProjectile>();
            trebuchetProj.EnableParticles();
            // Enable reload button
            trebuchetReloader.SetActive(true);
            yield break;
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

    protected virtual bool IsProjectileReadyToReleaseFromSling(float dotProductOfProjectileToWorldUp, float maxRandomDotProductOffset = 0.04f)
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

        projectile.GetComponent<Projectile>().allowCollisions = true;

        // Unparent projectile so trebuchet rotation doesn't affect it
        projectile.transform.SetParent(null);

        // TODO: Add some slight left/right deviation
        // ...
    }

    protected override bool CheckForLaunchInput()
    {
        // Physical button in game that will launch it
        return false;
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

        // Reset counterweight to 0 velocity and pointing straight down
        yield return ResetCounterweight();

        // Load another projectile into it
        yield return LoadProjectile();

        // Hold down the long arm end from launching the projectile
        yield return HoldDownArmFromLaunching();
    }

    private IEnumerator ResetCounterweight()
    {
        // Freeze weight rb
        counterweightRb.isKinematic = true;

        // Get hinge joint and axis to rotate around
        Vector3 hingeJointLocal = counterweightHingeJoint.anchor;
        Vector3 hingeJointWorld = counterweightHingeJoint.transform.TransformPoint(hingeJointLocal);
        Vector3 axis = counterweightHingeJoint.transform.right;

        // Target 1*
        float target = 1f;


        // Get opposite sign to make it travel in opposite dir (towards 0)
        float angle = counterweightRb.transform.localRotation.eulerAngles.x;
        float sign = Mathf.Sign(angle);
        // Angle speed is negative if angle < 180. Angle speed is positive otherwise.
        float angleSpeed = -1f;
        // If angle is > 180, target opposite angle (360 - target)
        if (Mathf.Abs(angle) > 180f)
        {
            target = 360f - target;
            // Make positive angle speed
            angleSpeed *= -1f;
        }

        // Rotate until euler.x of weight is close to 0 = close to neutral 0* position
        //  Run a timer just in case too; more than x seconds, just break out
        float end = 1f;
        float timer = 0f;
        float maxTimeToTake = 5f;
        // Euler angles always seems to be from 0 to 360. Find difference between target and current x euler angle.
        while(Mathf.Abs(target - counterweightRb.transform.localRotation.eulerAngles.x) > end && timer <= maxTimeToTake)
        {
            // Keep updating hingeJointWorld, in case rotating trebuchet while reloading
            hingeJointWorld = counterweightHingeJoint.transform.TransformPoint(counterweightHingeJoint.anchor);
            axis = counterweightHingeJoint.transform.right;

            counterweightRb.transform.RotateAround(hingeJointWorld, axis, angleSpeed);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        // Unfreeze rigidbody
        counterweightRb.isKinematic = false;

        // Set to default resting position
        counterweightRb.transform.localPosition = counterWeightLocalRestingPosition;
        counterweightRb.transform.localRotation = Quaternion.identity;

        // Remove any velocity on rigidbody
        counterweightRb.velocity = Vector3.zero;
        counterweightRb.angularVelocity = Vector3.zero;

        yield break;
    }

    private IEnumerator HoldDownArmFromLaunching()
    {
        // Let gravity take over counterweight
        counterweightRb.isKinematic = false;

        // Enable the thing holding the trebuchet
        trebuchetLaunchPreventer.SetActive(true);

        // Disable the reload button
        trebuchetReloader.SetActive(false);

        yield break;
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
        Vector3 axis = postHingeJoint.transform.right;
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

            // Keep updating in case rotating trebuchet while reloading
            hingeJointWorld = postHingeJoint.transform.TransformPoint(postHingeJoint.anchor);
            axis = postHingeJoint.transform.right;

            // Rotate arm around hinge post (ASSUMES FORWARD MATCHES Z-AXIS FORWARD)
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

        // Parent projectile to this trebuchet (in case trebuchet gets destroyed)
        projectile.transform.SetParent(gameObject.transform);

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
