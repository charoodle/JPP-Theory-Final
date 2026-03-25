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
///  TODO
///     <see cref="KeepYawBetween180(ref float)"/> - Uses hardcoded 360 to keep yaw inbetween [-180,180]. Change to modulo?
///  
///  See <see cref="MyProject.CharacterController.LookAt"/> and start there.
/// </summary>
public class RotationLookAt : MonoBehaviour
{
    #region Exposed Fields
    [Header("Debug")]
    [SerializeField] protected bool debugRayVisible = true;
    [SerializeField] protected float debugRayMaxDistance_FreedHead = 50f;
    [SerializeField] protected Color debugRayColor = Color.red;



    [Header("Fields")]
    /// <summary> Current yaw rotation. </summary>
    [SerializeField] protected float yawDegrees;
    /// <summary> Current pitch rotation (for the head). </summary>
    [SerializeField] protected float pitchDegrees;

    /// <summary> The body that rotates around (yaw only). </summary>
    [SerializeField] Transform rotateBody;
    /// <summary> The head that rotates around (pitch + yaw). </summary>
    [SerializeField] Transform rotateFreedHead;

    [Header("Settings")]
    /// <summary> How many degrees can look rotate head upwards </summary>
    private float maxPitchDegreesDown = -90f;
    /// <summary> How many degrees can look rotate head downwards </summary>
    private float maxPitchDegreesUp = 90f;
    #endregion


    #region LookAt Fields
    // Used to pass data between LookAt IEnumerators
    protected float lookAt_lastYawVel = 0f;
    protected float lookAt_lastPitchVel = 0f;

    // Parameter constants
    protected const float LOOKTIME = 0.5f;
    protected const float LOOKTIME_LERP = 1f;
    protected const float INITIAL_LOOKVEL = 0.5f;
    protected const float WITHIN_DEGREES = 2f;

    // Current look coroutine that is making the character lock their view onto something.
    protected Coroutine currentLookAt;
    #endregion


    #region LifeCycle Functions
    protected void LateUpdate()
    {
        // Look-around character
        CharacterLookAround(ref yawDegrees, ref pitchDegrees, rotateFreedHead, rotateBody);

        if(debugRayVisible)
        {
            // Head
            Debug.DrawRay(rotateFreedHead.transform.position, rotateFreedHead.transform.forward * debugRayMaxDistance_FreedHead, debugRayColor);
        }

    }
    #endregion


    #region LookAt Functions (aka stoppable midway) - not tested

    #endregion


    #region LookAt Functions (aka stoppable midway) - tested
    /// <summary>
    /// Look at a target forever until another LookAt function is called or <see cref="LookAtStop"/> is called.
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine"/>
    public void LookAt(Transform target, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        currentLookAt = StartCoroutine(LookAtPermanentlyCoroutine(target, lookTime, initialLookVel));
    }
    #endregion


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


    #region Main LookAt Coroutines
    /// <summary>
    /// Make the character controller permanently look at a target (until manually stopped) or this coroutine is called again.
    /// Uses the look rotation's pitch and yaw system to get a target pitch/yaw to smoothly rotate towards the target transform.
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine(Transform, float, float, float, float)"/>
    protected IEnumerator LookAtPermanentlyCoroutine(Transform target, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        // Cannot have negative look time.
        if (lookTime < 0)
        {
            Debug.LogWarning("LookAtCoroutine: Look time cannot be negative.");
            yield break;
        }

        float yawVel = initialLookVel;
        float pitchVel = initialLookVel;

        // TODO: If initialLookVel has opposite signage of yaw/pitch, then it can make it lerp the opposite way temporarily (even if no movement should happen)

        // Get target pitch and yaw from a world position for char to look at
        GetTargetPitchAndYawFrom(target.position, out float targetYaw, out float targetPitch);

        while (true)
        {
            // Target doesn't exist anymore; break out.
            if (!target)
                yield break;

            // Update target pitch and yaw, since target can be moving
            GetTargetPitchAndYawFrom(target.position, out targetYaw, out targetPitch);
            SmoothDampYawAndPitchToTarget(ref yawDegrees, ref pitchDegrees, targetYaw, targetPitch, ref yawVel, ref pitchVel, lookTime);
            yield return null;
        }
    }
    #endregion


    #region LookAt Helper Functions
    /// <summary>
    /// Converts the world position into a target yaw and pitch relative to this character's head.
    /// 
    /// TODO: Make the <see cref="rotateFreedHead"/> a passed-in parameter, Not a hardcoded value.
    /// </summary>
    /// 
    public void GetTargetPitchAndYawFrom(Vector3 worldPosition, out float yaw, out float pitch)
    {
        // Yaw and pitch degrees are relative to the world forward direction
        Vector3 worldForward = Vector3.forward;
        Vector3 headPosition = rotateFreedHead.transform.position;
        Vector3 toDirection = worldPosition - headPosition;
        Quaternion quat = Quaternion.FromToRotation(worldForward, toDirection);

        // Assign eulers out
        yaw = quat.eulerAngles.y;
        pitch = quat.eulerAngles.x;

        // If look object goes above head object, euler X will wrap around from 1* to 360*.
        if (quat.eulerAngles.x > 180f)
            pitch = quat.eulerAngles.x - 360f;

        // Make sure target yaw is within yaw system's degrees.
        KeepYawBetween180(ref yaw);
    }

    /// <summary>
    /// Stop the current LookAt coroutine.
    /// </summary>
    protected void StopLookAtCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    /// <summary>
    /// Meant to be used in a while loop. SmoothDamps yawDegrees and pitchDegrees towards targetYaw and targetPitch.
    /// </summary>
    /// <param name="yawDegrees">Character's current yaw rotation.</param>
    /// <param name="pitchDegrees">Character's current pitch rotation.</param>
    /// <param name="targetYaw">Target yaw for the character to look at.</param>
    /// <param name="targetPitch">Target pitch for the character to look at.</param>
    /// <param name="yawVel">Initial yaw velocity for Mathf.SmoothDamp.</param>
    /// <param name="pitchVel">Initial pitch velocity for Mathf.SmoothDamp.</param>
    /// <param name="lookTime">SmoothTime for Mathf.SmoothDamp.</param>
    protected void SmoothDampYawAndPitchToTarget(ref float yawDegrees, ref float pitchDegrees, float targetYaw, float targetPitch, ref float yawVel, ref float pitchVel, float lookTime)
    {
        float currentYaw = yawDegrees;
        float currentPitch = pitchDegrees;

        // Make sure the target angle has same system as this cc's yaw system.
        KeepYawBetween180(ref targetYaw);

        // Must use opposite version of yawDegrees angle if yawDegrees --> targetYaw crosses over from -180 to 180 (and vice versa).
        currentYaw = DetectIfYawPassesOver180(currentYaw, targetYaw);

        // SmoothDamp current yaw and pitch towards target yaw/pitch
        this.yawDegrees = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVel, lookTime);
        this.pitchDegrees = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVel, lookTime);

        // Convert to -180 to 180 for pitch system
        KeepYawBetween180(ref yawDegrees);
        // Pitch - Clamp from -90 to 90
        pitchDegrees = Mathf.Clamp(pitchDegrees, maxPitchDegreesDown, maxPitchDegreesUp);
    }

    /// <summary>
    /// Since using Vector3.Euler seems to put the character's Y rotation (yaw) from [-180, 180]. This yaw system constrains itself to that.
    /// </summary>
    /// <param name="yaw">Any angle to convert to between [-180,180].</param>
    protected void KeepYawBetween180(ref float yaw)
    {
        /// TODO: What if yaw is way bigger than 360? 540? Use modulo? Don't use hardcoded number like 360?
        if (yaw > 180f)
            yaw -= 360f; // get the equivalent negative version
        else if (yaw < -180f)
            yaw += 360f; // get the equivalent positive version
    }

    /// <summary>
    /// If yawDegrees and targetYaw are across 180/-180 degrees from each other, returns yawDegrees but as the same-sign version, so things like Mathf.Lerp/SmoothDamp don't suddenly jump values.
    /// </summary>
    /// <param name="yawDegrees">Current character's yaw degrees.</param>
    /// <param name="targetYaw">Target yaw degrees value.</param>
    /// <returns></returns>
    protected float DetectIfYawPassesOver180(float yawDegrees, float targetYaw)
    {
        // Must use opposite version of yawDegrees angle if yawDegrees --> targetYaw crosses over from -180 to 180 (and vice versa).
        // If targetYaw has a different sign than current one, make yawDegrees into equivalent negative version
        // Since it would cause it to lerp from 179.999 to -179.999 (making char spin all the way around)
        if (Mathf.Sign(targetYaw) != Mathf.Sign(yawDegrees))
        {
            // Yaw for this character controller follows euler angles and goes from -180 to 180 (where 0 = z-forward, and 180/-180 = z-back).
            // Breaking point between -180 and 180; must use opposite angles if crossing over the 180 mark from either direction.
            // Crossing over the 0 mark is perfectly fine.
            if (Mathf.Abs(yawDegrees) + Mathf.Abs(targetYaw) > 180f)
            {
                // Substitute yawDegrees with its opposite positive/negative equivalent.
                if (yawDegrees < 0f)
                {
                    yawDegrees = 360f + yawDegrees;
                }
                else if (yawDegrees >= 0)
                {
                    yawDegrees = (360f - yawDegrees) * -1f;
                }
            }
        }
        return yawDegrees;
    }
    #endregion
}
