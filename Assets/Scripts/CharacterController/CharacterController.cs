using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a character.
/// </summary>

namespace MyProject
{
    public abstract class CharacterController : MonoBehaviour
    {
        [SerializeField] protected UnityEngine.CharacterController controller;

        // Input
        Vector2 moveInput;
        Vector2 lookInput;

        // Movement
        [SerializeField] Vector3 playerVelocity;
        [SerializeField] protected float _walkSpeed = 3.0f;
        protected virtual float walkSpeed
        {
            get { return _walkSpeed; }
            set
            {
                // Cannot have negative speed
                if (value <= 0)
                {
                    Debug.LogError("Cannot have negative walk speed: " + value, gameObject);
                    value = 0;
                }
                _walkSpeed = value;
            }
        }
        [SerializeField] protected float sprintSpeedMultiplier = 2.0f;

        // Look-around
        [SerializeField] Transform rotateBody;
        [SerializeField] Transform rotateFreedHead;
        [SerializeField] protected float yawDegrees = 0f;
        [SerializeField] protected float pitchDegrees = 0f;
        [SerializeField] protected float maxPitchDegreesUp = 90f;         // how far character can look upwards
        [SerializeField] protected float maxPitchDegreesDown = -90f;      // how far character can look downwards

        // Jump
        [SerializeField] float jumpHeight = 3f;
        Vector3 worldGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] protected bool _isGrounded;
        public bool isGrounded
        {
            get { return _isGrounded; }
            protected set { _isGrounded = value; }
        }

        // Layers (for jumping)
        [SerializeField] LayerMask characterLayer;
        LayerMask groundCheckLayer;

        // Spawn position
        Vector3 spawnPosition;

        protected virtual void Start()
        {
            // Ground = anything not the character layer
            groundCheckLayer = ~characterLayer;

            // Original spawn position, in case ever fall out of map
            spawnPosition = transform.position;

            // Check player from going out of bounds every couple secons
            StartCoroutine(PreventOutOfBoundsCoroutine());
        }



        #region Look At Functions
        // Used to pass data between LookAt IEnumerators
        protected float lookAt_lastYawVel = 0f;
        protected float lookAt_lastPitchVel = 0f;

        // Param constants
        protected const float LOOKTIME = 0.5f;
        protected const float INITIAL_LOOKVEL = 0.5f;

        /*
         * =============== LookAt TODO ===============
         *  TODO: Pass a predicate in to lookat functions?
         *      Ex: Do look until...
         *          Within angle degrees
         *          Amount of time passes
         *          Player clicks button to exit dialogue or something
         *          
         *              Duplicate functions:
         *                  LookAtUntilWithinDegrees
         *                  LookAtPermanently
         *              Not (?) duplicate functions:
         *                  LookAtForTimePeriod (can branch to use previous yaw/pitch)
         */

        /// <summary>
        /// Make the character controller look towards a target for x seconds, and then optionally return to its previous look rotation.
        /// </summary>
        /// <param name="target">Target transform to look at.</param>
        /// <param name="timePeriod">How many seconds to maintain look at target.</param>
        /// <param name="withinDegrees">The minimum degree difference where it is considered acceptable enough to be "looking" at the target (SmoothDamp can take a long time to reach exact degrees).</param>
        /// <param name="returnBackToPrevLookDir">(Optionally) return to previous character's look direction.</param>
        /// <param name="lookTime">Roughly how many seconds until character's look direction matches to target direction.</param>
        /// <param name="initialLookVel">How fast the character look speed initially is.</param>
        protected virtual IEnumerator LookAtTargetForSecondsAndThenBack(Transform target, float timePeriod, float withinDegrees = 3f, bool returnBackToPrevLookDir = true, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
        {
            float savedPitch = pitchDegrees;
            float savedYaw = yawDegrees;

            // Look at the object
            yield return LookAtUntilWithinDegrees(target, withinDegrees, lookTime, initialLookVel);

            // Hold look there for a time period
            yield return LookAtForTimePeriod(target, 3f, lookTime, initialLookVel, true);

            // Return to previous rotation if desired.
            if (returnBackToPrevLookDir)
            {
                yield return LookAtTargetPitchYaw(savedPitch, savedYaw, withinDegrees, lookTime, initialLookVel);
            }
        }

        /// <summary>
        /// Make the character controller's view move towards a target for a time period (in seconds).
        /// </summary>
        /// <inheritdoc cref="LookAtTargetForSecondsAndThenBack(Transform, float, float, bool, float, float)"/>
        protected IEnumerator LookAtUntilWithinDegrees(Transform target, float withinDegrees, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
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
        /// <inheritdoc cref="LookAtTargetForSecondsAndThenBack(Transform, float, float, bool, float, float)"/>
        protected IEnumerator LookAtForTimePeriod(Transform target, float timePeriod, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL, bool useSavedYawPitchVelocity = false)
        {
            // Cannot have negative look time.
            if (lookTime < 0)
            {
                Debug.LogWarning("LookAtCoroutine: Look time cannot be negative.");
                yield break;
            }

            float yawVel;
            float pitchVel;
            if(useSavedYawPitchVelocity)
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
        /// Make the character controller permanently look at a target (until manually stopped).
        /// Uses the look rotation's pitch and yaw system to get a target pitch/yaw to smoothly rotate towards the target transform.
        /// </summary>
        /// <inheritdoc cref="LookAtTargetForSecondsAndThenBack(Transform, float, float, bool, float, float)"/>
        protected IEnumerator LookAtPermanently(Transform target, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
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
        /// Make the character controller rotate to look at a target pitch and yaw roughly within <paramref name="lookTime"/> seconds.
        /// </summary>
        /// <param name="targetPitch">Target pitch degrees to look at.</param>
        /// <param name="targetYaw">Target yaw degrees to look at.</param>
        /// <inheritdoc cref="LookAtTargetForSecondsAndThenBack(Transform, float, float, bool, float, float)"/>
        protected virtual IEnumerator LookAtTargetPitchYaw(float targetPitch, float targetYaw, float withinDegrees = 1.5f, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
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
        /// Converts the world position into a target yaw and pitch relative to character's head.
        /// </summary>
        protected void GetTargetPitchAndYawFrom(Vector3 worldPosition, out float yaw, out float pitch)
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



        #region Character Control
        protected virtual void Update()
        {
            // Update input
            bool jumpInput = false;
            bool sprintInput = false;
            UpdateInputs(ref moveInput, ref lookInput, ref jumpInput, ref sprintInput);
            
            // Move character
            MoveCharacter(moveInput, jumpInput, sprintInput, ref _isGrounded);

            // Update rotation (values only)
            UpdateLookRotation(lookInput, ref yawDegrees, ref pitchDegrees);
        }

        protected void LateUpdate()
        {
            // Look-around character
            CharacterLookAround(ref yawDegrees, ref pitchDegrees, rotateFreedHead, rotateBody);
        }

        protected virtual void UpdateInputs(ref Vector2 moveInput, ref Vector2 lookInput, ref bool jumpInput, ref bool sprintInput)
        {
            // Movement
            moveInput = GetMoveInput();
            jumpInput = GetJumpInput();
            sprintInput = GetSprintInput();

            // Look
            lookInput = GetLookInput();
            lookInput = ProcessLookInput(lookInput);
        }

        /// <summary>
        /// Process look input here after it's grabbed from some source. (ex: adjust by mouse sensitivity, invert directions, ...)
        /// </summary>
        /// <param name="lookInput"></param>
        /// <returns></returns>
        protected abstract Vector2 ProcessLookInput(Vector2 lookInput);

        protected void MoveCharacter(Vector3 moveInput, bool jumpInput, bool sprintInput, ref bool isGrounded)
        {
            // Update ground check
            isGrounded = IsGrounded();

            // Sprinting - modify final move speed
            float moveSpeed = walkSpeed;
            if (sprintInput)
                moveSpeed *= sprintSpeedMultiplier;

            // Move around (x and z values only)
            Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            controller.Move(moveDirection.normalized * Time.deltaTime * moveSpeed);

            // Reset player velocity while touching ground.
            if (isGrounded)
            {
                playerVelocity = Vector3.zero;
            }

            // Jump if on ground
            if (jumpInput && isGrounded)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * worldGravity.y);
            }

            // Apply gravity if not on ground
            if (!isGrounded)
                playerVelocity += worldGravity * Time.deltaTime;

            // Move with gravity (y value affected only)
            controller.Move(playerVelocity * Time.deltaTime);
        }

        /// <summary>
        /// Make the character game object move around
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
            detachedHead.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

            // Rotate player body to match camera view rotation
            body.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
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

        protected void UpdateLookRotation(Vector2 lookInput, ref float lookXRotation, ref float lookYRotation)
        {
            // Change x and y rotations by input
            lookXRotation += lookInput.x;
            lookYRotation -= lookInput.y;
        }

        /// <summary>
        /// Get move input. X for horizontal character movement. Y for forward/backward movement.
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 GetMoveInput();

        /// <summary>
        /// Get look input. X for yaw (left/right). Y for pitch (up/down).
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 GetLookInput();

        /// <returns>True if character wants to jump this frame. False otherwise.</returns>
        protected abstract bool GetJumpInput();

        /// <returns>True if character wants to sprint this frame. False otherwise.</returns>
        protected abstract bool GetSprintInput();
        
        /// <returns>True if character is touching ground (depends on CharacterController.skinWidth).</returns>
        protected bool IsGrounded()
        {
            // Ground = anything not on the player layer
            LayerMask groundCheckLayer = this.groundCheckLayer;

            // Account for the controller's skin width in raycast + a little more
            float maxDistance = controller.skinWidth + 0.02f;
            Vector3 characterFeet = transform.position;

            bool hitGround = false;
            if (Physics.Raycast(characterFeet, Vector3.down, out RaycastHit hitInfo, maxDistance, groundCheckLayer))
            {
                Debug.Log("Hit: " + hitInfo.collider.gameObject + " " + LayerMask.LayerToName(hitInfo.collider.gameObject.layer), hitInfo.collider.gameObject);
                hitGround = true;
            }

            // Green = is grounded, red = not grounded.
            //Color lineColor = hit ? Color.green : Color.red;

            // Debug draw the raycast line
            //Vector3 playerFeet = transform.position;
            //Debug.DrawLine(playerFeet, playerFeet + (Vector3.down * maxDistance), lineColor);

            return hitGround;
        }

        protected IEnumerator PreventOutOfBoundsCoroutine()
        {
            float checkSeconds = 5f;
            while (true)
            {
                if (transform.position.y < -30f)
                {
                    // Disable character controller to allow for movement
                    controller.enabled = false;
                    transform.position = spawnPosition;
                    controller.enabled = true;
                }

                yield return new WaitForSeconds(checkSeconds);
            }
        }
        #endregion
    }
}
