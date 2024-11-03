using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [SerializeField] protected float _destroyAfterSeconds = 5f;

    protected virtual void Start()
    {
        DestroyInAFewSeconds();
    }

    protected void DestroyInAFewSeconds()
    {
        Destroy(gameObject, _destroyAfterSeconds);
    }
}
