using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary> Used to tell scripts that this object is a castle. </summary>
public abstract class Castle : MonoBehaviour
{
    public delegate void HealthAction(Castle castle);
    public HealthAction OnCastleDestroyed;

    Health health;

    private void OnEnable()
    {
        health = GetComponent<Health>();
        if(health)
        {
            health.OnDie += OnCastleDie;
        }
    }

    private void OnDisable()
    {
        if(health)
        {
            health.OnDie -= OnCastleDie;
        }
    }

    protected virtual void OnCastleDie(float healthDummy)
    {
        // Receive health event, simplify call for listeners into an OnDestroyed event.
        OnCastleDestroyed?.Invoke(this);
    }
}
