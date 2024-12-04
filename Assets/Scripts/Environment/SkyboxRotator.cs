using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    Material skybox;
    [SerializeField] float speed;

    private void Start()
    {
        skybox = RenderSettings.skybox;
    }

    // Update is called once per frame
    void Update()
    {
        skybox.SetFloat("_Rotation", Time.time * speed);
    }
}
