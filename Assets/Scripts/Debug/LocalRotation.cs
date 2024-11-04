using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalRotation : MonoBehaviour
{
    [SerializeField] Vector3 localEuler;
    [SerializeField] Vector3 globalEuler;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        localEuler = transform.localRotation.eulerAngles;
        globalEuler = transform.rotation.eulerAngles;
    }
}
