using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Testing how to create an optional parameter for a Vector3. Can't use Vector3.zero. Must use: Vector3 foo = default(Vector3).
/// </summary>
public class DefaultVector3ParameterTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SomeMethod();
        SomeMethod(new Vector3(1,1.5f,-3.6f));

        float dummy = 0;
        SomeMethod1(ref dummy);

        SomeMethod2();
    }

    void SomeMethod(Vector3 param = default(Vector3))
    {
        Debug.Log("This is the default value: " + param);
    }

    void SomeMethod1(float defaultFloat0 = 3.5f, Vector3 v3 = default(Vector3))
    {
        Debug.Log("Vector3 optional parameter at end of parameter list.");
    }

    void SomeMethod1(ref float defaultFloat0, Vector3 v3 = default(Vector3))
    {
        Debug.Log("Vector3 optional parameter after ref.");
    }

    void SomeMethod2(Vector3 param = default)
    {
        Debug.Log("Intellisense says can simplify into this: " + param);
    }

    //void SomeMethod1(Vector3 v3 = default(Vector3), out float defaultFloat0)
    //{
    //    Debug.Log("Vector3 optional parameter cannot appear before ref or out.");
    //}
}
