using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Untested. Looks like it can be possible to pass in an object that you can modify in the while loop, such as a timer, or to check if an object is looking at another.
/// </summary>
public class Test_ExampleDelegate_LookAt : MonoBehaviour
{
    public delegate bool WaitUntilAction();
    // 1. While(timer < timeElapsed)
    // 2. While(isWithinDegrees)
    // 3. While(true)   //permanent

    protected void Bar()
    {
        WaitUntilAction action = () => true;
        StartCoroutine(Foo(action));
    }

    public class WaitUntilAction_Timer
    {
        public float timer = 0f;
        public float maxTimer = 5f;

        public bool IsConditionSatisfied()
        {
            return timer < maxTimer;
        }
    }

    public class WaitUntilAction_WithinDegrees
    {
        public Transform looker;
        public Transform target;

        public bool IsWithinDegrees()
        {
            // Do some degree check here
            return true;
        }
    }

    protected void Bar1()
    {
        WaitUntilAction_Timer timer = new WaitUntilAction_Timer();
        timer.timer = 0f;
        timer.maxTimer = 10f;
        WaitUntilAction action = timer.IsConditionSatisfied;
        StartCoroutine(Foo(action, timer: timer));
    }

    protected void Bar2()
    {
        WaitUntilAction_WithinDegrees withinDegrees = new WaitUntilAction_WithinDegrees();
        withinDegrees.looker = gameObject.transform;
        withinDegrees.target = GameObject.Find("Target").transform;

        WaitUntilAction action = withinDegrees.IsWithinDegrees;
        StartCoroutine(Foo(action, withinDegrees: withinDegrees));
    }

    protected IEnumerator Foo(WaitUntilAction condition, WaitUntilAction_Timer timer = null, WaitUntilAction_WithinDegrees withinDegrees = null)
    {

        while (condition())
        {
            // Increaser timer??
            if (timer != null)
            {
                timer.timer += Time.deltaTime;
            }

            // Permanent look at?
            // Nothing, condition handles it as () => true

            // Within degrees?
            // Nothing, condition handles it

            yield return null;
        }
    }
}
