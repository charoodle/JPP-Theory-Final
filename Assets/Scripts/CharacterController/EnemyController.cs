using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MyProject.CharacterController
{
    KnightAnimations knightAnimations;

    protected bool isSprinting = false;
    public bool canMove = true;
    public bool isAlliedNPC = false;

    /// <summary>
    /// TEMP. To be used to walk towards a castle. Currently used to target either enemy castle or player castle.
    /// </summary>
    public string castleTargetName;

    protected override float _WalkSpeed
    { 
        get => base._WalkSpeed;
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
        // Vary move speed for each enemy
        RandomizeBaseWalkSpeed(0.25f);

        // Make them start run
        knightAnimations = GetComponentInChildren<KnightAnimations>();
        if(knightAnimations && !isAlliedNPC) // Allied NPC henry cannot move; so default to idle
            knightAnimations.Run();

        base.Start();

        // Make them look at opposite enemy's castle. Must do this after base.Start() because the rot get initialized there.
        if (!isAlliedNPC)
        {
            GameObject playerCastle = GameObject.Find(castleTargetName);
            if (playerCastle)
                rot.LookAt(playerCastle.transform);
        }
    }

    /// <summary>Deviate walk speed by +- a percent of their current walk speed.</summary>
    /// <param name="deviationPct">Max percent [0f-1f] of current walk speed to add/subtract from base walk speed.</param>
    protected void RandomizeBaseWalkSpeed(float deviationPct)
    {
        // Pct goes from 0% - 100%
        deviationPct = Mathf.Clamp(deviationPct, 0f, 1f);
        // Speed up/slow down by a percent of walk speed
        float fractionOfWalkSpeed = deviationPct * _WalkSpeed;
        _WalkSpeed += Random.Range(-fractionOfWalkSpeed, fractionOfWalkSpeed);
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

    protected override Vector2 PostProcessLookInput(Vector2 lookInput)
    {
        // No extra processing of look input
        return lookInput;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PlayerCastle playerCastle = hit.gameObject.GetComponentInParent<Castle>() as PlayerCastle;
        // If an enemy touchs player castle, decrease its health
        if (playerCastle && !isAlliedNPC)
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
