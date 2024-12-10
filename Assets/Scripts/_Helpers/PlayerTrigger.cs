using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Slap a trigger onto this to have access to when the player enters/exits this trigger from its <see cref="TriggerAction"/> events.
/// </summary>
public class PlayerTrigger : MonoBehaviour
{
    public delegate void TriggerAction();
    public TriggerAction OnPlayerEnter;
    public TriggerAction OnPlayerExit;



    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = null;
        if(Utils.IsPlayer(other.gameObject, ref player))
        {
            OnPlayerEnter?.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = null;
        if (Utils.IsPlayer(other.gameObject, ref player))
        {
            OnPlayerExit?.Invoke();
        }
    }

    /// <summary>
    /// Enable/disable trigger.
    /// </summary>
    public void Enable(bool value)
    {
        gameObject.SetActive(value);
    }
}
