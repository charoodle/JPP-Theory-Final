using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialColorGetter : MonoBehaviour
{
    public Color currentColor;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Changing color");
        Renderer renderer = other.GetComponent<Renderer>();
        currentColor = renderer.material.GetColor("_Color");
    }

    public static Color GetColor(Renderer renderer)
    {
        return renderer.material.GetColor("_Color");
    }
}
