using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;

/// <summary>
/// For testing out <see cref="RotationLookAt"/> functions while extracting them out from <see cref="MyProject.CharacterController"/>.
/// </summary>
public class LookAtFunctions_Extracted_Tests : MonoBehaviour
{
    [SerializeField] RotationLookAt obj;

    public List<GameObject> targets;
    public KeyCode nextTargetKey = KeyCode.Space;
    public KeyCode test_IEnums_Key = KeyCode.I;
    protected int lastTargetIdx;
    public float LookAt_WithinDegreesAmt = 5f;

    Coroutine coroutine;

    void Update()
    {
        if (Input.GetKeyDown(nextTargetKey))
            Test_LookAt(GetRandomTarget());

        if (Input.GetKeyDown(test_IEnums_Key))
            Test_LookAt_IEnums();
    }

    protected void Test_LookAt(GameObject lookAtTarget)
    {
        if (lookAtTarget == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        //obj.LookAt(target.transform);
        //obj.LookAtUntilWithinDegrees(target.transform, LookAt_WithinDegreesAmt);
        //obj.LookAtTargetForSeconds(target.transform, 1f, LookAt_WithinDegreesAmt);
        //StartCoroutine(LookAtTargetForSecondsThenSwitchToRandomTarget(target));
        //obj.LookAtTargetPitchYaw(30f, 180f);
        obj.LookAt_TargetPitchYaw_Lerp(30f, 180f, 5f);
    }


    protected void Test_LookAt_IEnums()
    {
        if(coroutine != null)
            StopCoroutine(coroutine);
        coroutine = StartCoroutine(Test_LookAt_IEnums_CR());
    }

    protected IEnumerator Test_LookAt_IEnums_CR()
    {
        Transform target = GetRandomTarget().transform;

        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookAt_TargetForSeconds(target, 3f), "LookAtTargetForSecondsEnum");

        target = GetRandomTarget().transform;
        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookAt_UntilWithinDegrees(target, 1f), "LookAtUntilWithinDegreesEnum");

        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookToward_UntilTimePeriod(target, 5f), "LookTowardUntilTimePeriodEnum");

        target = GetRandomTarget().transform;
        float yaw, pitch;
        obj.Get_LookAt_TargetPitchAndYawFrom(target.position, out yaw, out pitch);
        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookAt_TargetPitchYaw_Lerp(pitch, yaw, 5f), "LookAtTargetPitchYaw_LerpEnum");

        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookAt_TargetPitchYaw(pitch, yaw), "LookAtTargetPitchYaw");

        Debug.Log("(Done) Finished all tests!");

        yield break;


        /// Tests various LookAtEnum functions for timing
        IEnumerator Test_LookAtIEnumFunction(Func<IEnumerator> lookAtFunction, string funcName)
        {
            float startPitch = 0f;
            float startYaw = 0f;
            obj.Get_LookAt_YawAndPitchDegrees(out startPitch, out startYaw);

            yield return new WaitForSeconds(1f);

            // Keep track of timer
            Debug.Log($"(START) {funcName}");
            float startTime = Time.time;
            
            // Run the LookAt function provided
            yield return lookAtFunction();

            // Output the final pitch/yaw
            float pitch, yaw = 0;
            obj.Get_LookAt_YawAndPitchDegrees(out pitch, out yaw);

            // How long did it take
            Debug.Log($"(FINISHED!) Time elapsed: {GetTimeElapsed(startTime)} " +
                $"\nFinal Pitch: {pitch}d | Final Yaw: {yaw}d");

            // Return to beginning pitch/yaw values
            yield return obj.Enum_LookAt_TargetPitchYaw(startPitch, startYaw, withinDegrees: 0.05f);

            DebugPrintLine();
        }
        
        static string GetTimeElapsed(float startTime, int decimalPlaces = 2)
        {
            return (Mathf.Round((Time.time - startTime) * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces) + " seconds");
        }

        void DebugPrintLine()
        {
            Debug.Log("----------------------------------------------");
        }
    }

    protected IEnumerator LookAtTargetForSecondsThenSwitchToRandomTarget(GameObject target)
    {
        yield return obj.Enum_LookAt_TargetForSeconds(target.transform, 1f, LookAt_WithinDegreesAmt);

        obj.LookAt(GetRandomTarget().transform);
    }

    protected GameObject GetRandomTarget()
    {
        if (targets == null || targets.Count <= 0)
            return null;

        int idx = Random.Range(0, targets.Count);
        while (targets.Count > 1 && idx == lastTargetIdx)
        {
            idx = Random.Range(0, targets.Count);
        }

        // Update last target to prevent duplicate for next time
        lastTargetIdx = idx;
        return targets[idx];
    }
}
