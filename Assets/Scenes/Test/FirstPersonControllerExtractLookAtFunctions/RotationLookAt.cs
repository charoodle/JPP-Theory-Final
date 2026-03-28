using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO:
///     [x] Port all functions as-is with same names and functions.
///     [x] Make sure it all works with a psuedo head/body, like how an enemy should behave.
///         
///  TODO when done porting:
///     Rename and clean up functions for a single generic object. Not just for a character-controller lookaround. 
///         - [ ] <see cref="INITIAL_LOOKVEL"/>: explain more. Which function(s) can returns a lookVel value?
///         - [ ] Rename LookAt functions to be easier to differentiate. Use underscores, like: "LookAt_X" / "LookAt_Y."
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

    /// <summary> How many degrees can look rotate head upwards. </summary>
    [Header("Settings")]
    private float maxPitchDegreesDown = -90f;
    /// <summary> How many degrees can look rotate head downwards. </summary>
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

    /// <summary>
    /// Look at a target until the pitch/yaw degrees reach a certain degrees.
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine"/>
    public void LookAtUntilWithinDegrees(Transform target, float withinDegrees, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        currentLookAt = StartCoroutine(LookAtUntilWithinDegreesCoroutine(target, withinDegrees, lookTime, initialLookVel));
    }

    /// <summary>
    /// Look toward a target until pitch/yaw of view is <paramref name="withinDegrees"/> of target.
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine"/>
    public void LookAtTargetForSeconds(Transform target, float timePeriod, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        currentLookAt = StartCoroutine(LookAtTargetForSecondsEnum(target, timePeriod, withinDegrees, lookTime, initialLookVel));
    }

    /// <summary>
    /// Look towards a pitch/yaw value. Can get the current pitch/yaw value to use this method later with <see cref="GetYawAndPitchDegrees(out float, out float)"/>.
    /// </summary>
    /// <inheritdoc cref="LookAtTargetPitchYawCoroutine(float, float, float, float, float)"/>
    public void LookAtTargetPitchYaw(float targetPitch, float targetYaw, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        currentLookAt = StartCoroutine(LookAtTargetPitchYawCoroutine(targetPitch, targetYaw, withinDegrees, lookTime, initialLookVel));
    }

    /// <summary>Make the character look towards a target pitch/yaw within <paramref name="lookTime"/> seconds exactly (uses Lerp instead of SmoothDamp).</summary>
    /// <inheritdoc cref="LookAtTargetPitchYawLerpCoroutine"/>
    public void LookAtTargetPitchYaw_Lerp(float targetPitch, float targetYaw, float lookTime)
    {
        StopLookAtCoroutine(currentLookAt);
        currentLookAt = StartCoroutine(LookAtTargetPitchYawLerpCoroutine(targetPitch, targetYaw, lookTime));
    }

    /// <summary>
    /// Stop the current pausable LookAt coroutine. Does not work with any of the "LookAt___Enum" versions, since those must be played out until their conditions are satisfied.
    /// </summary>
    public void LookAtStop()
    {
        // No couroutine exists.
        if (currentLookAt == null)
        {
            Debug.LogWarning("Nothing to stop looking at.", this.gameObject);
            return;
        }

        StopLookAtCoroutine(currentLookAt);
    }

    /// <summary>
    /// Get the character controller's current pitch/yaw rotation in degrees.
    /// </summary>
    /// <param name="pitch">X axis rotation of character view. Goes from <see cref="maxPitchDegreesDown"/> to <see cref="maxPitchDegreesUp"/>. </param>
    /// <param name="yaw">Y axis rotation of character view. Goes from -180f - 180f. (Based on <see cref="Quaternion.Euler"/>)</param>
    public void GetYawAndPitchDegrees(out float pitch, out float yaw)
    {
        pitch = pitchDegrees;
        yaw = yawDegrees;
    }

    /// <summary>
    /// Look towards a target for <paramref name="timePeriod"/> seconds total. Time starts ticking the moment the function is called.
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine(Transform, float, float, float, float)"></inheritdoc>
    public void LookTowardUntilTimePeriod(Transform target, float timePeriod, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        currentLookAt = StartCoroutine(LookTowardUntilTimePeriodCoroutine(target, timePeriod, lookTime, initialLookVel, useSavedYawPitchVelocity: false));
    }

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
    #endregion


    #region LookAt IEnum Functions (aka unstoppable midway)
    /// <summary>
    /// Public coroutine version of <see cref="LookAtTargetForSeconds"/>.
    /// <para> Warning: You must keep track of this coroutine by yourself. It does not have safeguards to stop itself if you forget about it running.</para>
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSeconds"/>
    public IEnumerator LookAtTargetForSecondsEnum(Transform target, float timePeriod, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        yield return LookAtTargetForSecondsCoroutine(target, timePeriod, withinDegrees, lookTime, initialLookVel);
    }

    /// <summary>
    /// Public coroutine version of <see cref="LookAtUntilWithinDegrees"/>
    /// <para> Warning: You must keep track of this coroutine by yourself. It does not have safeguards to stop itself if you forget about it running.</para>
    /// </summary>
    /// <inheritdoc cref="LookAtUntilWithinDegrees"/>
    public IEnumerator LookAtUntilWithinDegreesEnum(Transform target, float withinDegrees, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        yield return LookAtUntilWithinDegreesCoroutine(target, withinDegrees, lookTime, initialLookVel);
    }

    /// <summary>
    /// Public coroutine version of <see cref="LookTowardUntilTimePeriod"/>
    /// <para> Warning: You must keep track of this coroutine by yourself. It does not have safeguards to stop itself if you forget about it running.</para>
    /// </summary>
    /// <inheritdoc cref="LookTowardUntilTimePeriod"/>
    public IEnumerator LookTowardUntilTimePeriodEnum(Transform target, float timePeriod, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        yield return LookTowardUntilTimePeriodCoroutine(target, timePeriod, lookTime, initialLookVel, useSavedYawPitchVelocity: false);
    }

    /// <summary>
    /// Public coroutine version of <see cref="LookAtTargetPitchYaw(float, float, float, float, float)"/>
    /// <para> Warning: You must keep track of this coroutine by yourself. It does not have safeguards to stop itself if you forget about it running.</para>
    /// </summary>
    /// <inheritdoc cref="LookAtTargetPitchYaw"/>
    public IEnumerator LookAtTargetPitchYawEnum(float targetPitch, float targetYaw, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        StopLookAtCoroutine(currentLookAt);
        yield return LookAtTargetPitchYawCoroutine(targetPitch, targetYaw, withinDegrees, lookTime, initialLookVel);
    }

    /// <summary>
    /// Public coroutine version of <see cref="LookAtTargetPitchYaw(float, float, float, float, float)"/>
    /// <para> Warning: You must keep track of this coroutine by yourself. It does not have safeguards to stop itself if you forget about it running.</para>
    /// </summary>
    /// <param name="lookTime">The exact time in seconds to Lerp towards the target pitch/yaw.</param>
    /// <inheritdoc cref="LookAtTargetPitchYaw"/>
    public IEnumerator LookAtTargetPitchYaw_LerpEnum(float targetPitch, float targetYaw, float lookTime)
    {
        StopLookAtCoroutine(currentLookAt);
        yield return LookAtTargetPitchYawLerpCoroutine(targetPitch, targetYaw, lookTime);
    }
    #endregion


    #region (Protected) LookAt Coroutines
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

    /// <summary>
    /// Make the character controller's view move towards a target for a time period (in seconds).
    /// </summary>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine(Transform, float, float, float, float)"></inheritdoc>
    protected IEnumerator LookAtUntilWithinDegreesCoroutine(Transform target, float withinDegrees, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        // Degrees should be positive
        withinDegrees = Mathf.Abs(withinDegrees);

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

        while (!LookDegreesIsCloseEnough(yawDegrees, targetYaw, withinDegrees) || !LookDegreesIsCloseEnough(pitchDegrees, targetPitch, withinDegrees))
        {
            // Target doesn't exist anymore; break out.
            if (!target)
                yield break;

            // Update target pitch and yaw, since target can be moving
            GetTargetPitchAndYawFrom(target.position, out targetYaw, out targetPitch);
            SmoothDampYawAndPitchToTarget(ref yawDegrees, ref pitchDegrees, targetYaw, targetPitch, ref yawVel, ref pitchVel, lookTime);
            yield return null;
        }

        // Keep current yaw/pitch velocity and pass it out in case there's a LookAtForTimePeriod coroutine chained after this that needs it.
        LookAt_SaveCurrentYawPitchVelocity(yawVel, pitchVel);
    }

    /// <summary>
    /// Make the character controller's view move towards a target for a time period (in seconds).
    /// </summary>
    /// <param name="timePeriod">How many seconds to move view towards target, no matter the current view yaw/pitch.</param>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine(Transform, float, float, float, float)"/>
    protected IEnumerator LookTowardUntilTimePeriodCoroutine(Transform target, float timePeriod, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL, bool useSavedYawPitchVelocity = false)
    {
        // Cannot have negative look time.
        if (lookTime < 0)
        {
            Debug.LogWarning("LookAtCoroutine: Look time cannot be negative.");
            yield break;
        }

        // Dont need to run through this code.
        if (lookTime == 0)
        {
            yield break;
        }

        float yawVel;
        float pitchVel;
        if (useSavedYawPitchVelocity)
        {
            yawVel = lookAt_lastYawVel;
            pitchVel = lookAt_lastPitchVel;
        }
        else
        {
            yawVel = initialLookVel;
            pitchVel = initialLookVel;
        }

        // TODO: If initialLookVel has opposite signage of yaw/pitch, then it can make it lerp the opposite way temporarily (even if no movement should happen)

        // Get target pitch and yaw from a world position for char to look at
        GetTargetPitchAndYawFrom(target.position, out float targetYaw, out float targetPitch);

        float timer = 0f;
        while (timer < timePeriod)
        {
            // Target doesn't exist anymore; break out.
            if (!target)
                yield break;

            // Update target pitch and yaw, since target can be moving
            GetTargetPitchAndYawFrom(target.position, out targetYaw, out targetPitch);
            SmoothDampYawAndPitchToTarget(ref yawDegrees, ref pitchDegrees, targetYaw, targetPitch, ref yawVel, ref pitchVel, lookTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Make the character controller look towards a target until its within a certain degrees, keeps looking towards it for x seconds, and then stops.
    /// </summary>
    /// <param name="target">Target transform to look at.</param>
    /// <param name="timePeriod">How many seconds to maintain look at target.</param>
    /// <param name="withinDegrees">The minimum degree difference where it is considered acceptable enough to be "looking" at the target (SmoothDamp can take a long time to reach exact degrees).</param>
    /// <param name="lookTime">Roughly how many seconds until character's look direction matches to target direction.</param>
    /// <param name="initialLookVel">How fast the character look speed initially is.</param>
    protected virtual IEnumerator LookAtTargetForSecondsCoroutine(Transform target, float timePeriod, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        // Look at the object
        yield return LookAtUntilWithinDegreesCoroutine(target, withinDegrees, lookTime, initialLookVel);

        // Hold look there for a time period
        bool usePreviousLookAtVelocity = true;
        yield return LookTowardUntilTimePeriodCoroutine(target, timePeriod, lookTime, initialLookVel, usePreviousLookAtVelocity);
    }

    /// <summary>
    /// Make the character controller rotate to look at a target pitch and yaw roughly within <paramref name="lookTime"/> seconds.
    /// </summary>
    /// <param name="targetPitch">Target pitch degrees to look at.</param>
    /// <param name="targetYaw">Target yaw degrees to look at.</param>
    /// <inheritdoc cref="LookAtTargetForSecondsCoroutine(Transform, float, float, float, float)"/>
    protected virtual IEnumerator LookAtTargetPitchYawCoroutine(float targetPitch, float targetYaw, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
    {
        // Cannot have negative look time.
        if (lookTime < 0)
        {
            Debug.LogWarning("LookAtCoroutine: Look time cannot be negative.");
            yield break;
        }

        // Cannot divide by 0 in lerp function. Skips loop.
        if (lookTime == 0)
        {
            this.pitchDegrees = targetPitch;
            this.yawDegrees = targetYaw;
            yield break;
        }

        float yawVel = initialLookVel;
        float pitchVel = initialLookVel;

        // TODO: If initialLookVel has opposite signage of yaw/pitch, then it can make it lerp the opposite way temporarily (even if no movement should happen)

        while (!LookDegreesIsCloseEnough(yawDegrees, targetYaw, withinDegrees) || !LookDegreesIsCloseEnough(pitchDegrees, targetPitch, withinDegrees))
        {
            SmoothDampYawAndPitchToTarget(ref yawDegrees, ref pitchDegrees, targetYaw, targetPitch, ref yawVel, ref pitchVel, lookTime);
            yield return null;
        }

        // When done, make sure to snap character rotation to target rotation.
        pitchDegrees = targetPitch;
        yawDegrees = targetYaw;
    }

    /// <summary>
    /// Make the character controller rotate to look at a target pitch and yaw exactly within <paramref name="lookTime"/> seconds.
    /// </summary>
    /// <param name="lookTime">Exactly how many seconds until character's look direction will match target direction.</param>
    /// <inheritdoc cref="LookAtTargetPitchYawCoroutine(float, float, float, float, float)"/>
    protected virtual IEnumerator LookAtTargetPitchYawLerpCoroutine(float targetPitch, float targetYaw, float lookTime = LOOKTIME_LERP)
    {
        // Cannot have negative look time.
        if (lookTime < 0)
        {
            Debug.LogWarning("LookAtCoroutine: Look time cannot be negative.");
            yield break;
        }

        // Cannot divide by 0 in lerp function. Skips loop.
        if (lookTime == 0)
        {
            this.pitchDegrees = targetPitch;
            this.yawDegrees = targetYaw;
            yield break;
        }

        // TODO: If initialLookVel has opposite signage of yaw/pitch, then it can make it lerp the opposite way temporarily (even if no movement should happen)

        float timer = 0f;
        float startYaw = yawDegrees;
        float startPitch = pitchDegrees;
        while (timer <= lookTime)
        {
            float pct = timer / lookTime;
            LerpYawAndPitchToTarget(ref yawDegrees, ref pitchDegrees, targetYaw, targetPitch, startYaw, startPitch, pct);
            timer += Time.deltaTime;
            yield return null;
        }

        // When done, make sure to snap character rotation to target rotation.
        pitchDegrees = targetPitch;
        yawDegrees = targetYaw;
    }
    #endregion


    #region LookAt Helper Functions
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

    /// <param name="pct">Lerp percent (0f-1f).</param>
    /// <inheritdoc cref="SmoothDampYawAndPitchToTarget(ref float, ref float, float, float, ref float, ref float, float)"/>
    protected void LerpYawAndPitchToTarget(ref float yawDegrees, ref float pitchDegrees, float targetYaw, float targetPitch, float startYaw, float startPitch, float pct)
    {
        // Make sure the target angle has same system as this cc's yaw system.
        KeepYawBetween180(ref targetYaw);

        // Must use opposite version of yawDegrees angle if yawDegrees --> targetYaw crosses over from -180 to 180 (and vice versa).
        startYaw = DetectIfYawPassesOver180(startYaw, targetYaw);

        // Lerp current yaw and pitch towards target yaw/pitch
        this.yawDegrees = Mathf.Lerp(startYaw, targetYaw, pct);
        this.pitchDegrees = Mathf.Lerp(startPitch, targetPitch, pct);

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

    /// <returns>True if the degree difference between yaw and targetYaw is within the degree gap. False if too big.</returns>
    protected bool LookDegreesIsCloseEnough(float yawDegrees, float targetYawDegrees, float degreesGap = 0.1f)
    {
        return Mathf.Abs(targetYawDegrees - yawDegrees) <= Mathf.Abs(degreesGap);
    }

    /// <summary>
    /// Cache the yawVel and pitchVel. Should be used when chaining coroutine LookAtForTimePeriod after LookAtUntilWithinDegrees.
    /// </summary>
    protected void LookAt_SaveCurrentYawPitchVelocity(float yawVel, float pitchVel)
    {
        lookAt_lastYawVel = yawVel;
        lookAt_lastPitchVel = pitchVel;
    }
    #endregion
}
