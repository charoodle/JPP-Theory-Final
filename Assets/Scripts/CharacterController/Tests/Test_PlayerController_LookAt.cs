using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_PlayerController_LookAt : PlayerController
{
    protected Transform target;
    protected override void Update()
    {
        // 1-2: Switch targets
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            target = GameObject.Find("LookAtPosition1").transform;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            target = GameObject.Find("LookAtPosition2").transform;
        }

        // J: Look at target for some seconds
        if(Input.GetKeyDown(KeyCode.J))
        {
            rot.LookAt_TargetForSeconds(target, 2f);
        }

        // K: Look towards target for seconds
        if (Input.GetKeyDown(KeyCode.K))
        {
            rot.LookToward_UntilTimePeriod(target, 3f);
        }

        // L: Look at target permanently
        if(Input.GetKeyDown(KeyCode.L))
        {
            rot.LookAt(target);
        }

        // I: Look to the right 90*
        if(Input.GetKeyDown(KeyCode.I))
        {
            rot.LookAt_TargetPitchYaw(0f, 90f);
        }

        // O: Look at target until within degrees
        if(Input.GetKeyDown(KeyCode.O))
        {
            rot.LookAt_UntilWithinDegrees(target, 0.1f);
        }

        // P: Stop look at.
        if(Input.GetKeyDown(KeyCode.P))
        {
            rot.LookAtStop();
        }

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    Transform enemyCastle = GameObject.Find("EnemyCastle").transform;
        //    StartCoroutine(LookAtTargetForSecondsAndThenBack(enemyCastle, 5f));
        //}

        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    Transform target = GameObject.Find("LookAtPosition").transform;
        //    LookAtPermanently(target);
        //}

        //if (Input.GetKeyDown(KeyCode.J))
        //{
        //    StopAllCoroutines();
        //}

        base.Update();
    }
}
