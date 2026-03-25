using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO:
///     [ ] Port all functions as-is with same names and functions.
///     [ ] Make sure it works with a psuedo head/body, like how an enemy should behave.
///         
///  TODO when done porting:
///     Rename and clean up functions for a single generic object. Not just for a character-controller lookaround.
///     Rework the detached head/body situation.
///     Make into a portable, reusable script.
///     
///  TODO when done making portable and reusable:
///     Refactor CharacterController to use the portable script instead.
///     Refactor CharacterController to use in BulletPain project.
///  
///  See <see cref="MyProject.CharacterController.LookAt"/> and start there.
/// </summary>
public class RotationLookAt : MonoBehaviour
{
    [SerializeField] protected float yawDegrees;
    [SerializeField] protected float pitchDegrees;
    [SerializeField] Transform rotateBody;
    [SerializeField] Transform rotateFreedHead;

    [Header("Settings")]
    /// <summary> How many degrees can look rotate head upwards </summary>
    private float maxPitchDegreesDown = -90f;
    /// <summary> How many degrees can look rotate head downwards </summary>
    private float maxPitchDegreesUp = 90f;

    /// <summary>
    /// Make the character game object look around
    /// </summary>
    /// <param name="yawDegrees">Euler degrees to pitch the yaw around.</param>
    /// <param name="pitchDegrees">Euler degrees to pitch the character around.</param>
    /// <param name="detachedHead">Rotates around the y *and* x axis. So it should be on a separate gameobject than the body (ex camera).</param>
    /// <param name="body">Rotates around the y axis.</param>
    protected void CharacterLookAround(ref float yawDegrees, ref float pitchDegrees, Transform detachedHead, Transform body)
    {
        // Clamp up and down rotation
        pitchDegrees = Mathf.Clamp(pitchDegrees, maxPitchDegreesDown, maxPitchDegreesUp);

        // Keep look rotation within -180 to +180. If goes over 180 or less than -180, wrap it by 360* and change the sign.
        KeepYawBetween180(ref yawDegrees);

        // Rotate camera - assumes its on a separate object from character that follow's character's body
        if (detachedHead)
            detachedHead.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

        // Rotate player body to match camera view rotation
        if (body)
            body.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
    }

    protected void LateUpdate()
    {
        // Look-around character
        CharacterLookAround(ref yawDegrees, ref pitchDegrees, rotateFreedHead, rotateBody);
    }

    /// <summary>
    /// Since using Vector3.Euler seems to put the character's Y rotation (yaw) from [-180, 180]. This character controller's yaw system constrains itself to that.
    /// </summary>
    /// <param name="yaw">Any angle to convert to between [-180,180].</param>
    protected void KeepYawBetween180(ref float yaw)
    {
        if (yaw > 180f)
            yaw -= 360f; // get the equivalent negative version
        else if (yaw < -180f)
            yaw += 360f; // get the equivalent positive version
    }
}
