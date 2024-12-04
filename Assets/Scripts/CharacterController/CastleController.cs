using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleController : MyProject.CharacterController
{
    /// <summary>
    /// To only allow one move at a time. Ex: Move/Rotate buttons get disabled if a move happens.
    /// Buttons reenabled when the move is finished.
    /// </summary>
    public delegate void CastleMoveAction();
    public event CastleMoveAction OnStartMove;
    public event CastleMoveAction OnStopMove;

    protected Vector2 castleMoveInput;

    public enum MoveDir
    {
        /// <summary>
        /// Forward
        /// </summary>
        Forw,

        /// <summary>
        /// Backward
        /// </summary>
        Back,

        Left,
        Right
    }

    public void MoveCastle(MoveDir dir)
    {
        switch(dir)
        {
            case (MoveDir.Left):
                Move(Vector2.left);
                break;
            case (MoveDir.Right):
                Move(Vector2.right);
                break;
            case (MoveDir.Forw):
                Move(Vector2.up);
                break;
            case (MoveDir.Back):
                Move(Vector2.down);
                break;
        }
    }

    public void TurnCastle(MoveDir dir)
    {
        Turn(dir);
    }

    protected void Move(Vector2 dir)
    {
        float duration = 3f;
        StartCoroutine(MoveEnum(dir, duration));
    }

    protected void Turn(MoveDir dir)
    {
        // Add/subtract degrees from current yaw degrees if want to turn left/right
        const float degreesToTurn = 45f;
        // Determine degree signage by direction
        float degreeDiff = 0f;
        if (dir == MoveDir.Left)
            degreeDiff = -degreesToTurn;
        else if (dir == MoveDir.Right)
            degreeDiff = degreesToTurn;
        // Determine the target degrees to turn to
        float currYawDegrees = yawDegrees;
        float targetYawDegrees = currYawDegrees + degreeDiff;

        // Make sure angle is within -180 to 180 euler system
        KeepYawBetween180(ref targetYawDegrees);

        // Do the turn
        const float seconds = 5f;
        StartCoroutine(TurnEnum(targetYawDegrees, seconds));
    }

    protected IEnumerator MoveEnum(Vector2 moveDir, float seconds = 3f)
    {
        OnStartMove?.Invoke();

        // Change moveInput around
        castleMoveInput = moveDir;

        yield return new WaitForSeconds(seconds);

        // Change back to 0
        castleMoveInput = Vector2.zero;

        OnStopMove?.Invoke();
    }

    /// <summary>
    /// Turn the castle on the y axis (yaw).
    /// </summary>
    /// <param name="targetYawDegrees"></param>
    /// <param name=""></param>
    /// <returns></returns>
    protected IEnumerator TurnEnum(float targetYawDegrees, float secondsToTurn)
    {
        OnStartMove?.Invoke();

        // Lerp castle rotation from current rotation to target rotation.
        //  Does not change pitch.
        yield return LookAtTargetPitchYaw_LerpEnum(pitchDegrees, targetYawDegrees, secondsToTurn);

        OnStopMove?.Invoke();
    }

    protected override Vector2 GetMoveInput()
    {
        return castleMoveInput;
    }

    #region Unused CharacterController Overrides(tentative)
    protected override bool GetJumpInput()
    {
        // Castles can never jump
        return false;
    }

    protected override Vector2 GetLookInput()
    {
        // Castles can't look around (yet)
        return Vector2.zero;
    }

    protected override bool GetSprintInput()
    {
        // Castles can't sprint
        return false;
    }

    protected override Vector2 ProcessLookInput(Vector2 lookInput)
    {
        // Not needed?
        return lookInput;
    }
    #endregion
}
