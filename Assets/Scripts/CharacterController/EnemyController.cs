using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MyProject.CharacterController
{
    protected bool isSprinting = false;
    public bool canMove = true;

    protected override float walkSpeed
    { 
        get => base.walkSpeed;
        set
        {
            // Enemies cannot have speed <= 1
            if (value <= 1)
            {
                Debug.LogWarning("Enemy cannot have walkspeed <= 1: " + value, gameObject);
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
        RandomizeBaseWalkSpeed(0.25f);

        // Make them look at player castle
        GameObject playerCastle = GameObject.Find("PlayerCastle");
        LookAtPermanently(playerCastle.transform);

        base.Start();
    }

    /// <summary>Deviate walk speed by +- a percent of their current walk speed.</summary>
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
        Vector2 moveInput = Vector2.zero;

        // Move forward
        if (canMove)
            moveInput = new Vector2(0f, 1f);

        return moveInput;
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
        PlayerCastle playerCastle = hit.gameObject.GetComponentInParent<Castle>() as PlayerCastle;
        // If touch player castle, decrease its health
        if (playerCastle)
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
            health.TakeDamage(10f);
            return true;
        }
        return false;
    }
}
