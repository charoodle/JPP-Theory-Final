using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// holy shit that's how you can pass in IEnumerator as a parameter? I might be able to refactor the RotationLookAt stuff after all...
/// 
/// Can pass in different IEnumerator functions
/// </summary>
public class FuncIEnumerator_Example : MonoBehaviour
{
    public bool StartTest = false;
    Coroutine cr;

    void OnValidate()
    {   
        if(StartTest)
        {
            StartTest = false;

            if (cr != null)
                    StopCoroutine(cr);
            cr = StartCoroutine(TestIEnum());
        }
    }

    IEnumerator TestIEnum()
    {
        Debug.Log("Starting IEnumerators...");
        yield return TimeLookAtFunction(test1);
        yield return TimeLookAtFunction(() => test2(10f));
        Debug.Log("Finished IEnumerators!");

        // The last one in a list of Func<...> is the return value. So this returns an IEnumerator with no input parameters (?)
        //  However, if you use a lambda, you can pass in the parameters for the IEnumerator/Coroutine
        IEnumerator TimeLookAtFunction(Func<IEnumerator> lookAtMethod)
        {
            yield return lookAtMethod();
            yield return new WaitForSeconds(1f);

            yield break;
        }

        IEnumerator test1()
        {
            Debug.Log("This is the first IEnumerator!");
            yield break;
        }

        IEnumerator test2(float asdf)
        {
            Debug.Log("This is the 2nd IEnumerator! With a parameter: " + asdf.ToString());
            yield break;
        }
    }
}
