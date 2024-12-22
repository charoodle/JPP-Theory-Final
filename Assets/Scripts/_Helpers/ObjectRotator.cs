using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    [SerializeField] float speed = 20f;
    
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, Time.time * speed, 0f);
    }
}
