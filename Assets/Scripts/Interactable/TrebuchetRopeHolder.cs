using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetRopeHolder : UnityEventInteractable
{
    public void Cut()
    {
        gameObject.SetActive(false);
    }

    public void Reappear()
    {
        gameObject.SetActive(true);
    }
}
