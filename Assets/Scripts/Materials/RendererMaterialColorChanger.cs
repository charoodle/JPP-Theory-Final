using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererMaterialColorChanger : MonoBehaviour
{
    [SerializeField] Renderer[] renderers;

    public void ChangeColor(Color color)
    {
        foreach(Renderer rend in renderers)
        {
            rend.material.color = color;
        }
    }
}
