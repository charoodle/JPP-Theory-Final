using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetTemp : MonoBehaviour
{
    public Rigidbody weightRb;
    public GameObject projectile;

    public Rigidbody mainArmRb;
    public GameObject mainArm;

    public HingeJoint postHingeJoint;

    public Transform reloadHingeArmAttachPoint;
    public Transform projectileSpawnPoint;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            // Release weight
            weightRb.isKinematic = false;
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            // Launch projectile
            HingeJoint projectileToArmHinge = projectile.GetComponent<HingeJoint>();
            Destroy(projectileToArmHinge);
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ReloadTrebuchet());
        }
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

    private IEnumerator ReduceRigidbodyToZeroVelocity(Rigidbody rb, float lerpPct)
    {
        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, lerpPct);
        yield break;
    }

    [Range(0f,1f)]
    [SerializeField] float armPctRotation;
    private IEnumerator ResetEntireArmToNeutralRotation(float neutralXRotation = 0f)
    {
        /*
         * Main arm moves secondary and sling arm through hinge joints
         * Main arm rb is frozen at this point
         * 
         * 
         * Make Main arm rotate around left post's anchor axis
         */

        // Make weight free so it doesn't look so weird when resetting arm
        weightRb.isKinematic = false;

        // Convert from local hinge joint anchor to point in world
        Vector3 hingeJointLocal = postHingeJoint.anchor;
        Vector3 hingeJointWorld = postHingeJoint.transform.TransformPoint(hingeJointLocal);
        Vector3 axis = Vector3.right;   //x axis
        float angleSpeed = -1f;  // rotate backwards

        // TEST: Rotate around the position for x seconds
        Debug.Log("Beginning to rotate around hinge point...");
        float time = 0.0f;
        float duration = 5.0f;

        float origMainArmRotation = mainArm.transform.localRotation.eulerAngles.x;
        float targetMainArmXRotation = 3f;

        Debug.Log($"Start: {origMainArmRotation}. End: {targetMainArmXRotation}");

        while (mainArm.transform.localRotation.eulerAngles.x > targetMainArmXRotation)
        {
            // How many pct is the rotation complete so far, from orig position to target position?
            //  - subtract by origMainArmRotation because it would otherwise offset the percentage calculation?
            //  - problem: euler angles have wonky values?
            armPctRotation = (mainArm.transform.localRotation.eulerAngles.x - origMainArmRotation) / (targetMainArmXRotation - origMainArmRotation);
            Debug.Log("Percent:" + armPctRotation);

            // Make the weight go towards 0 velocity anyways, even with wonky percent numbers
            yield return ReduceRigidbodyToZeroVelocity(weightRb, armPctRotation);

            // Rotate arm around hinge post
            mainArm.transform.RotateAround(hingeJointWorld, axis, angleSpeed);

            yield return new WaitForFixedUpdate();
        }

        while(time < duration)
        {
            time += Time.fixedDeltaTime;
            // Stop if arm reaches zero'd rotation (default arm position)
            //  - MainArm is child of Arm, so if MainArm's local rotation = 0, that means it's the default rotation it was first given
            //  - localRotation goes from 0-360 only, so try to catch both cases? (this seems pretty bad)
            if (mainArm.transform.localRotation.eulerAngles.x <= 5 || mainArm.transform.localRotation.eulerAngles.x >= 350f)
            {
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Finished rotating around hinge joint.");

        // Freeze weight to prepare for next shot
        weightRb.isKinematic = true;

        yield break;

        // --------

        // Make arm rotation go to default
        /*
        Debug.Log("Bringing arm to neutral rotation...");
        float time = 0.0f;
        float duration = 5f;
        GameObject arm = mainArmRb.gameObject;
        Quaternion armRotation = arm.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(neutralXRotation, 0f, 0f);
        while (time < duration)
        {
            time += Time.deltaTime;

            // How much time has elapsed as a percent
            float pct = time / duration;

            // Rotate arm's x axis towards netural rotation
            //arm.transform.localRotation = Quaternion.Lerp(armRotation, targetRotation, pct);

            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Done! Brought arm to neutral position.");

        yield break;
        */
    }

    private IEnumerator LoadProjectile()
    {
        yield break;
    }
}
