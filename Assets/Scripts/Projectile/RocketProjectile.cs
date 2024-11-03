using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : Projectile
{
    [SerializeField] float _speed;
    float speed
    {
        get
        {
            return _speed;
        }
        set
        {
            if (value <= 0)
                Debug.LogError($"Cannot have rocket projectile speed set to <= 0: value={value}");
            _speed = value;
        }
    }

    private void FixedUpdate()
    {
        // Make it go forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // TODO: Make it go slow, lock onto target, then speed up and follow it
        //  TODO: Change direction?
        //transform.TransformDirection(Vector3.forward * speed * Time.deltaTime);
    }

    public void InitializeRocket(float launchForce)
    {
        speed = launchForce;
    }

    protected override void DestroyInAFewSeconds()
    {
        destroyAfterSeconds = 10f;
        base.DestroyInAFewSeconds();
    }
}
