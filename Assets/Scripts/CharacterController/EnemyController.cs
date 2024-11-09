using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MyProject.CharacterController
{
    protected bool isSprinting = false;

    protected override float walkSpeed
    { 
        get => base.walkSpeed;
        set
        {
            // Enemies cannot have speed <= 1
            if (value <= 1)
            {
                Debug.LogError("Enemy cannot have walkspeed <= 1: " + value, gameObject);
                value = 1;
            }
            _walkSpeed = value;
        }
    }

    protected override void Start()
    {
        // Make enemy rotation retain same way they are facing on game start.
        yawDegrees = transform.rotation.eulerAngles.y;

        // Vary move speed for each enemy
        RandomizeBaseWalkSpeed(1f);

        base.Start();
    }

    /// <param name="deviationPct">Max percent [0f-1f] of current walk speed to add/subtract from base walk speed.</param>
    protected void RandomizeBaseWalkSpeed(float deviationPct)
    {
        // Pct goes from 0% - 100%
        deviationPct = Mathf.Clamp(deviationPct, 0f, 1f);
        // Speed up/slow down by a percent of walk speed
        float fractionOfWalkSpeed = deviationPct * walkSpeed;
        walkSpeed += Random.Range(-fractionOfWalkSpeed, fractionOfWalkSpeed);
    }

    protected override bool GetJumpInput()
    {
        // No jumping
        return false;
    }

    protected override Vector2 GetLookInput()
    {
        // No changing look direction
        return Vector2.zero;
    }

    protected override Vector2 GetMoveInput()
    {
        // Move forward
        return new Vector2(0f, 1f);
    }

    protected override bool GetSprintInput()
    {
        // No sprinting
        return false;
    }

    protected override Vector2 ProcessLookInput(Vector2 lookInput)
    {
        // No extra processing of look input
        return lookInput;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // If touch player castle, decrease its health
        if (hit.transform.root.gameObject.name == "PlayerCastle")
        {
            // Castle takes damage
            TakeHealthAwayFrom(hit);

            // Destroy this enemy
            Destroy(gameObject);
        }
    }

    protected virtual bool TakeHealthAwayFrom(ControllerColliderHit collision)
    {
        // Take health away from object when hit with this projectile
        //  Colliders usually on child objects. Scripts on parent objects.
        Health health = collision.gameObject.GetComponentInParent<Health>();
        if (health)
        {
            health.TakeDamage(10.0005f);
            return true;
        }
        return false;
    }
}
