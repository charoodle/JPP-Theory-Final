using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [SerializeField] protected float _destroyAfterSeconds = 5f;
    [SerializeField] protected float destroyAfterSeconds
    {
        get
        {
            return _destroyAfterSeconds;
        }
        set
        {
            if (value <= 0)
                Debug.LogError($"Projectile cannot have destroyedAfterSeconds with value <= 0. value={value}");
            _destroyAfterSeconds = value;
        }
    }

    protected virtual void Start()
    {
        DestroyInAFewSeconds();
    }

    protected virtual void DestroyInAFewSeconds()
    {
        Destroy(gameObject, destroyAfterSeconds);
    }
}
