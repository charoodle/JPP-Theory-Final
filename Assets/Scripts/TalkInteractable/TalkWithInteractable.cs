using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TalkWithInteractable : Interactable
{
    [SerializeField] protected string _npcName;
    protected string npcName
    {
        get { return _npcName; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.LogError("NPC Name cannot be empty.");
                return;
            }
            _npcName = value;
        }
    }

    public override void InteractWith()
    {
        TalkWith();
    }

    protected virtual void TalkWith()
    {
        Debug.Log("Talk with: " + npcName);
        StartCoroutine(TalkWithCoroutine());
    }

    protected abstract IEnumerator TalkWithCoroutine();
}
