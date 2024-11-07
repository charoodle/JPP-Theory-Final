using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MyProject.CharacterController
{
    protected override void Start()
    {
        // Make enemy rotation retain same way they are facing on game start.
        yawDegrees = transform.rotation.eulerAngles.y;
        base.Start();
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
        return Vector2.zero;
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
}
