using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_PlayerController_LookAt : PlayerController
{
    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Transform enemyCastle = GameObject.Find("EnemyCastle").transform;
            StartCoroutine(LookAtTargetForSecondsAndThenBack(enemyCastle, 5f));
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            Transform target = GameObject.Find("LookAtPosition").transform;
            LookAtPermanently(target);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            StopAllCoroutines();
        }

        base.Update();
    }
}
