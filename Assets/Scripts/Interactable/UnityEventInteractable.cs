using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventInteractable : Interactable
{
    public UnityEvent unityEvent;

    public override void InteractWith()
    {
        unityEvent.Invoke();
    }
}
