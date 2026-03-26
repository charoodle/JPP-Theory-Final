using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For testing out <see cref="RotationLookAt"/> functions while extracting them out from <see cref="CharacterController"/>.
/// </summary>
public class LookAtFunctions_Extracted_Tests : MonoBehaviour
{
    [SerializeField] RotationLookAt obj;
    public List<GameObject> targets;
    public KeyCode nextTargetKey = KeyCode.Space;
    protected int lastTargetIdx;

    public float LookAt_WithinDegreesAmt = 5f;

    void Update()
    {
        if (Input.GetKeyDown(nextTargetKey))
            LookAt(GetRandomTarget());
    }

    protected void LookAt(GameObject target)
    {
        if(target == null)
        {
            Debug.LogError("Target is null.");
            return;
        }

        //obj.LookAt(target.transform);
        obj.LookAtUntilWithinDegrees(target.transform, LookAt_WithinDegreesAmt);
    }

    protected GameObject GetRandomTarget()
    {
        if (targets == null || targets.Count <= 0)
            return null;

        int idx = Random.Range(0, targets.Count);
        while(targets.Count > 1 && idx == lastTargetIdx)
        {
            idx = Random.Range(0, targets.Count);
        }

        return targets[idx];
    }
}
