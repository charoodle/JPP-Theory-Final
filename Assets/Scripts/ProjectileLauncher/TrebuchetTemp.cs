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
            StartCoroutine(MoveArmToDefaultRotation());
        }
    }

    private IEnumerator MoveArmToDefaultRotation()
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

        // Reset main arm to neutral rotation
        yield return ResetEntireArmToNeutralRotation();

        yield break;
    }

    private IEnumerator ReduceRigidbodyToZeroVelocity(Rigidbody rb, float lerpPct)
    {
        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, lerpPct);
        yield break;
    }

    [SerializeField] Vector3 mainArmEulers;
    private IEnumerator ResetEntireArmToNeutralRotation(float neutralXRotation = 0f)
    {
        /*
         * Main arm moves secondary and sling arm through hinge joints
         * Main arm rb is frozen at this point
         * 
         * 
         * Make Main arm rotate around left post's anchor axis
         */

        Vector3 hingeJointLocal = postHingeJoint.anchor;

        // Convert from local hinge joint anchor to point in world
        Vector3 hingeJointWorld = postHingeJoint.transform.TransformPoint(hingeJointLocal);
        Vector3 axis = Vector3.right;   //x axis
        float angle = -1f;  // rotate backwards

        // TEST: Rotate around the position for x seconds
        Debug.Log("Beginning to rotate around hinge point...");
        float time = 0.0f;
        float duration = 5.0f;
        while(time < duration)
        {
            time += Time.fixedDeltaTime;

            // Rotate arm
            mainArm.transform.RotateAround(hingeJointWorld, axis, angle);

            mainArmEulers = mainArm.transform.localRotation.eulerAngles;
            // Stop if arm reaches zero'd rotation (default arm position)
            if (mainArm.transform.localRotation.eulerAngles.x <= 0 || mainArm.transform.localRotation.eulerAngles.x >= 350f)
            {
                Debug.Log("Arm is at zero rotation, breaking out of loop.");
                yield break;
            }

            yield return new WaitForFixedUpdate();
        }
        Debug.Log("Finished rotating around hinge joint.");

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
}
