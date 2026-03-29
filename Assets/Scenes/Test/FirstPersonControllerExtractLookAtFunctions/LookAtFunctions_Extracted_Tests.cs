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
    public KeyCode test_PublicCalls_Key = KeyCode.J;
    protected int lastTargetIdx;
    protected int currentTargetIdx;
    public float LookAt_WithinDegreesAmt = 5f;

    Coroutine coroutine;

    void Update()
    {
        //if (Input.GetKeyDown(nextTargetKey))
        //    Test_LookAt(GetRandomTarget());

        if (Input.GetKeyDown(test_IEnums_Key))
            Test_LookAt_IEnums();

        if (Input.GetKeyDown(test_PublicCalls_Key))
            Test_LookAt_PublicCalls();
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

        Vector3 offset = new Vector3(5f, 0f, 0f);
        coroutine = StartCoroutine(Test_LookAt_IEnums_CR(offset));
    }

    protected void Test_LookAt_PublicCalls()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(Test_LookAt_CR());
    }

    protected IEnumerator Test_LookAt_CR()
    {
        Vector3 offset = new Vector3(0f, 3f, 0f);

        Transform target = GetNextTarget().transform;
        yield return Test_LookAt_Function(() => obj.LookAt(target, targetOffset: offset), "LookAt");

        target = GetNextTarget().transform;
        yield return Test_LookAt_Function(() => obj.LookAt_TargetForSeconds(target, 3f, targetOffset: offset), "LookAt_TargetForSeconds");

        target = GetNextTarget().transform;
        yield return Test_LookAt_Function(() => obj.LookAt_UntilWithinDegrees(target, 3f, targetOffset: offset), "LookAt_UntilWithinDegrees");

        target = GetNextTarget().transform;
        yield return Test_LookAt_Function(() => obj.LookToward_UntilTimePeriod(target, 5f, targetOffset: offset), "LookToward_UntilTimePeriod");

        Debug.Log("(Done) Finished all tests!");


        IEnumerator Test_LookAt_Function(Action lookAtFunction, string funcName, KeyCode continueButton = KeyCode.Space)
        {
            float startPitch = 0f;
            float startYaw = 0f;
            obj.Get_LookAt_YawAndPitchDegrees(out startPitch, out startYaw);

            yield return new WaitForSeconds(1f);

            // Keep track of timer
            Debug.Log($"(START) {funcName}");
            float startTime = Time.time;

            // Run the LookAt function provided
            lookAtFunction();

            // Wait for user input before proceeding on
            yield return WaitForButtonPress(continueButton);

            // Output the current pitch/yaw
            float pitch, yaw = 0;
            obj.Get_LookAt_YawAndPitchDegrees(out pitch, out yaw);

            // Output final results. Time elapsed and pitch/yaw may not be as accurate.
            Debug.Log($"(FINISHED!) Time elapsed: {GetTimeElapsed(startTime)} " +
                $"\nFinal Pitch: {pitch}d | Final Yaw: {yaw}d");

            DebugPrintLine();
        }

        IEnumerator WaitForButtonPress(KeyCode keycode)
        {
            yield return new WaitUntil(() => { return Input.GetKeyDown(keycode); });
        }
    }

    protected IEnumerator Test_LookAt_IEnums_CR(Vector3 offset)
    {
        Transform target = GetRandomTarget().transform;

        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookAt_TargetForSeconds(target, 3f, targetOffset:offset), "LookAtTargetForSecondsEnum");

        target = GetRandomTarget().transform;
        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookAt_UntilWithinDegrees(target, 1f, targetOffset: offset), "LookAtUntilWithinDegreesEnum");

        yield return Test_LookAtIEnumFunction(() => obj.Enum_LookToward_UntilTimePeriod(target, 5f, targetOffset: offset), "LookTowardUntilTimePeriodEnum");

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
    }

    protected IEnumerator LookAtTargetForSecondsThenSwitchToRandomTarget(GameObject target)
    {
        yield return obj.Enum_LookAt_TargetForSeconds(target.transform, 1f, LookAt_WithinDegreesAmt);

        obj.LookAt(GetRandomTarget().transform);
    }

    protected GameObject GetNextTarget()
    {
        if (targets == null || targets.Count <= 0)
            return null;
        int targetIdx = currentTargetIdx % targets.Count;
        currentTargetIdx++;
        return targets[targetIdx];
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

    static string GetTimeElapsed(float startTime, int decimalPlaces = 2)
    {
        return (Mathf.Round((Time.time - startTime) * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces) + " seconds");
    }

    void DebugPrintLine()
    {
        Debug.Log("----------------------------------------------");
    }
}
