using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetTemp : MonoBehaviour
{
    public GameObject projectilePrefab;
    public GameObject projectile;
    public Transform projectileSpawnPoint;
    public Transform reloadHingeArmAttachPoint;
    public Rigidbody weightRb;
    public Rigidbody slingArmRb;
    public Rigidbody mainArmRb;
    public GameObject mainArm;
    public HingeJoint postHingeJoint;

    // Debug
    [Range(0f, 1f)]
    [SerializeField] float armPctRotation;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(ShootTrebuchet());
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadTrebuchet());
        }
    }

    private IEnumerator ShootTrebuchet()
    {
        // Release weight
        weightRb.isKinematic = false;

        // Check for when point from ball to arm exceeds roughly 90* (dot product?)
        Vector3 armUp = Vector3.up;

        // Calculate a random point after 1 to release for some randomness
        float randomness = Random.Range(0, 0.04f);
        bool readyToRelease = false;
        while(true)
        {
            Vector3 armToBall = (projectile.transform.position - mainArm.transform.position).normalized;
            Debug.DrawRay(mainArm.transform.position, armToBall * 3f, Color.red);
            Debug.DrawRay(mainArm.transform.position, armUp * 3f, Color.green);

            // -1 at rest
            // 0 when orthogonal
            // 1 at launch
            float dot = Vector3.Dot(armUp, armToBall);

            // Release hinge joint automatically
            // Ball is near highest point; get ready to release
            if (dot >= 0.95f && !readyToRelease)
            {
                readyToRelease = true;
            }

            // Release ball before highest point. Break out of infinite loop.
            //  - dot product is at least at .95, so it should release before dot product hits 1 (90*)
            if(readyToRelease && dot < 1 - randomness)
            {
                ReleaseSling();
                yield break;
            }

            Debug.Log(dot);
            yield return null;
        }
    }

    private void ReleaseSling()
    {
        // Launch projectile
        HingeJoint projectileToArmHinge = projectile.GetComponent<HingeJoint>();
        Destroy(projectileToArmHinge);

        // Add some slight left/right deviation
        // ...
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
        Debug.Log("Bringing weight and arm to zero velocity...");
        float time = 0.0f;
        float duration = 5f;
        while (time < duration)
        {
            time += Time.deltaTime;

            // How much time has elapsed as a percent
            float pct = time / duration;

            // Reduce arm and weight velocities to 0
            yield return ReduceRigidbodyToZeroVelocity(weightRb, pct);
            yield return ReduceRigidbodyToZeroVelocity(mainArmRb, pct);

            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Done! Brought weight and arm to zero velocity.");

        // Freeze rigidbodies
        mainArmRb.isKinematic = true;
        weightRb.isKinematic = true;
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
        weightRb.isKinematic = false;

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
            //      - this isn't accurate
            armPctRotation = (mainArm.transform.localRotation.eulerAngles.x - origMainArmRotation) / (targetMainArmXRotation - origMainArmRotation);

            // Make the weight go towards 0 velocity anyways, even with wonky percent numbers
            yield return ReduceRigidbodyToZeroVelocity(weightRb, armPctRotation);

            // Rotate arm around hinge post
            mainArm.transform.RotateAround(hingeJointWorld, axis, angleSpeed);

            yield return new WaitForFixedUpdate();
        }

        // Freeze weight to prepare for next shot
        weightRb.isKinematic = true;

        yield break;
    }

    /// <summary>
    /// Load and prepare a projectile to be shot again.
    /// </summary>
    private IEnumerator LoadProjectile()
    {
        // Can unfreeze main arm. Relies on unfreezing weight to fire.
        mainArmRb.velocity = Vector3.zero;
        mainArmRb.isKinematic = false;

        // Make sure weight is frozen
        weightRb.isKinematic = true;

        // Spawn another ball at projectile spawn point
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);

        // Move hinge joint anchor (local) of ball to reloadHingeArmAttachPoint's position (world)
        //  -- get world position as local position relative to the ball
        HingeJoint hingejoint = projectile.GetComponent<HingeJoint>();
        Vector3 armAttachPointLocalToProjectile = projectile.transform.InverseTransformPoint(reloadHingeArmAttachPoint.transform.position);
        hingejoint.anchor = armAttachPointLocalToProjectile;

        // Connect hingejoint to the trebuchet's sling arm
        hingejoint.connectedBody = slingArmRb;

        // Assign projectile
        this.projectile = projectile;

        yield break;
    }
}
