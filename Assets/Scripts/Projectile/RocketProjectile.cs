using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : Projectile
{
    /// <summary>
    /// Spawned on collision with something.
    /// </summary>
    [SerializeField] GameObject rocketExplosionPrefab;
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
    protected bool alreadyExploded = false;

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

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // Not an enemy
        if(!Utils.IsEnemy(collision.gameObject))
        {
            // Spawn explosion prefab, with up direction facing collision's normal direction (if its not an enemy)
            ContactPoint point = collision.GetContact(0);
            CreateExplosion(point.point, point.normal);
        }
        else
        {
            // Spawn explosion prefab, with up direction facing world up
            ContactPoint point = collision.GetContact(0);
            CreateExplosion(point.point, Vector3.up);
        }
    }

    protected void CreateExplosion(Vector3 position, Vector3 up)
    {
        // Only explode once
        if (alreadyExploded)
            return;

        alreadyExploded = true;
        GameObject explosion = Instantiate(rocketExplosionPrefab, position, rocketExplosionPrefab.transform.rotation);
        explosion.transform.up = up;
    }
}
