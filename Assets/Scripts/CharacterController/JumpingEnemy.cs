using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpingEnemy : EnemyController
{
    [SerializeField] bool wantsToJump = false;

    // Random cooldown time
    const float minJumpCooldownTime = 2f;
    const float maxJumpCooldownTime = 6f;
    [Range(0f, maxJumpCooldownTime)]
    [SerializeField] float jumpCooldownSecondsLeft = 0.0f;

    protected override void Start()
    {
        StartCoroutine(JumpEveryCoupleSeconds());
        base.Start();
    }

    protected IEnumerator JumpEveryCoupleSeconds()
    {
        while(true)
        {
            // Jump as soon as on ground and cooldown is over
            yield return new WaitUntil(() => isGrounded);

            // Jump the moment it can jump
            wantsToJump = true;

            // Wait for update to see if not grounded
            yield return null;

            // If no longer on ground, then jump probably happened. Wait a few seconds before next jump.
            if(!isGrounded)
            {
                wantsToJump = false;
                // Cooldown time
                float cooldown = Random.Range(minJumpCooldownTime, maxJumpCooldownTime);
#if UNITY_EDITOR
                StartCoroutine(DebugShowCooldownTimerInInspector(cooldown));
#endif
                yield return new WaitForSeconds(cooldown);
            }
        }
    }

#if UNITY_EDITOR
    protected IEnumerator DebugShowCooldownTimerInInspector(float cooldownSeconds)
    {
        jumpCooldownSecondsLeft = cooldownSeconds;
        while(true)
        {
            yield return null;

            jumpCooldownSecondsLeft -= Time.deltaTime;

            if(jumpCooldownSecondsLeft <= 0)
            {
                jumpCooldownSecondsLeft = 0f;
                yield break;
            }
        }
    }
#endif

    protected override bool GetJumpInput()
    {
        return wantsToJump;
    }
}
