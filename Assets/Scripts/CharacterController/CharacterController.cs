using System.Collections;
using UnityEngine;

/// <summary>
/// Controls a character.
/// </summary>

namespace MyProject
{
    public abstract class CharacterController : MonoBehaviour
    {
        #region Events
        public delegate void CharacterAction();
        /// <summary>
        /// Happens when the character jumps.
        /// </summary>
        public event CharacterAction OnCharacterJump;
         
        /// <summary>
        /// Happens when the character lands.
        /// </summary>
        public event CharacterAction OnCharacterLand;
        #endregion


        [SerializeField] protected UnityEngine.CharacterController controller;
        /// <summary>
        /// Property for <see cref="controller"/>, for the (Unity) character controller component.
        /// </summary>
        public UnityEngine.CharacterController charController
        {
            get
            {
                return controller;
            }
            protected set
            {
                controller = value;
            }
        }

        /// <summary>
        /// Is there a higher root object to this character controller?
        /// Will be parented/unparented when they walk on surfaces.
        /// </summary>
        [SerializeField] Transform rootGameObject;
        /// <summary>
        /// Original root object on game start. Used to parent/unparent this object back and forth from to this transform (ex: ground checks).
        /// </summary>
        protected Transform originalRoot;

        // Input
        Vector2 _moveInput;
        Vector2 _lookInput;
        protected bool _sprintInput = false;

        /// <summary>
        /// The current character's move input.
        /// </summary>
        public Vector2 moveInput
        {
            get { return _moveInput; }
            protected set { _moveInput = value; }
        }

        /// <summary>
        /// The current character's look input.
        /// </summary>
        public Vector2 lookInput
        {
            get { return _lookInput; }
            protected set { _lookInput = value; }
        }

        /// <summary>
        /// Is the character holding the sprint button while moving in a direction?
        /// </summary>
        public bool isSprinting { get { return _sprintInput && moveInput.magnitude > 0; } }

        // Movement
        /// <summary>
        /// TODO: This only works with gravity. Does not update X/Z velocity.
        /// </summary>
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
        /// <summary>
        /// Flag to manage allowing one jump command per accepted character jump inpu. This should initially be true on game start to let the character jump.
        /// <para>Triggered by <see cref="WaitForCharacterToLandOnGround"/>, and is used in the jump section of <see cref="MoveCharacter(Vector3, bool, bool, ref bool)"/></para>
        /// </summary>
        protected bool isGroundedAndCanJumpAgain = true;

        // Movement with a parent
        /// <summary>
        /// Is the character controller currently grounded on something that can move? May need to supply additional movement to the CharacterController.Move() function.
        /// </summary>
        [SerializeField] MovableGroundSurface currentMovingGroundSurface;
        [SerializeField] Vector3 currentGroundVelocity;
        [SerializeField] Vector3 lastTouchedGroundVelocity;

        // Layers (for jumping)
        [SerializeField] LayerMask characterLayer;
        LayerMask groundCheckLayer;

        // Spawn position
        Vector3 spawnPosition;

        /// <summary>
        /// The look at head of the character. Useful for aim target during dialogue.
        /// </summary>
        public Transform head
        {
            get { return rotateFreedHead; }
        }

        protected virtual void Start()
        {
            // Ground = anything not the character layer
            groundCheckLayer = ~characterLayer;

            // Original spawn position, in case ever fall out of map
            spawnPosition = transform.position;

            // Check player from going out of bounds every couple secons
            StartCoroutine(PreventOutOfBoundsCoroutine());

            // Get one layer above the root game object as the original root. Assumes (!!!) char controller is only one layer deep (?).
            if (rootGameObject)
            {
                originalRoot = rootGameObject.transform.parent;
            }

            // Make look system update to same rotation as object's initial starting rotation
            yawDegrees = transform.rotation.eulerAngles.y;
        }



        #region Character Look At

        #region LookAt Fields
        // Used to pass data between LookAt IEnumerators
        protected float lookAt_lastYawVel = 0f;
        protected float lookAt_lastPitchVel = 0f;

        // Param constants
        protected const float LOOKTIME = 0.5f;
        protected const float LOOKTIME_LERP = 1f;
        protected const float INITIAL_LOOKVEL = 0.5f;
        protected const float WITHIN_DEGREES = 2f;

        // Current look coroutine that is making the character lock their view onto something.
        protected Coroutine currentLookAt;
        #endregion

        #region LookAt Functions (aka stoppable midway)
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
        #endregion

        #region LookAt IEnum Functions (aka unstoppable midway)
        /// <summary> Public coroutine version of <see cref="LookAtTargetForSeconds"/></summary>
        /// <inheritdoc cref="LookAtTargetForSeconds"/>
        public IEnumerator LookAtTargetForSecondsEnum(Transform target, float timePeriod, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
        {
            StopLookAtCoroutine(currentLookAt);
            yield return LookAtTargetForSecondsCoroutine(target, timePeriod, withinDegrees, lookTime, initialLookVel);
        }

        /// <summary> Public coroutine version of <see cref="LookAtUntilWithinDegrees"/></summary>
        /// <inheritdoc cref="LookAtTargetForSeconds"/>
        public IEnumerator LookAtUntilWithinDegreesEnum(Transform target, float withinDegrees, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
        {
            StopLookAtCoroutine(currentLookAt);
            yield return LookAtUntilWithinDegreesCoroutine(target, withinDegrees, lookTime, initialLookVel);
        }

        /// <summary> Public coroutine version of <see cref="LookTowardUntilTimePeriod"/></summary>
        /// <inheritdoc cref="LookAtTargetForSeconds"/>
        public IEnumerator LookTowardUntilTimePeriodEnum(Transform target, float timePeriod, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
        {
            StopLookAtCoroutine(currentLookAt);
            yield return LookTowardUntilTimePeriodCoroutine(target, timePeriod, lookTime, initialLookVel, useSavedYawPitchVelocity: false);
        }

        /// <summary> Public coroutine version of <see cref="LookAtTargetPitchYaw(float, float, float, float, float)"/></summary>
        /// <inheritdoc cref="LookAtTargetForSeconds"/>
        public IEnumerator LookAtTargetPitchYawEnum(float targetPitch, float targetYaw, float withinDegrees = WITHIN_DEGREES, float lookTime = LOOKTIME, float initialLookVel = INITIAL_LOOKVEL)
        {
            StopLookAtCoroutine(currentLookAt);
            yield return LookAtTargetPitchYawCoroutine(targetPitch, targetYaw, withinDegrees, lookTime, initialLookVel);
        }

        public IEnumerator LookAtTargetPitchYaw_LerpEnum(float targetPitch, float targetYaw, float lookTime)
        {
            StopLookAtCoroutine(currentLookAt);
            yield return LookAtTargetPitchYawLerpCoroutine(targetPitch, targetYaw, lookTime);
        }
        #endregion

        #region Main LookAt Coroutines
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

        /// <summary>
        /// Stop the current LookAt coroutine.
        /// </summary>
        protected void StopLookAtCoroutine(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        #endregion

        #endregion



        #region Character Control
        protected virtual void Update()
        {
            // Update input
            bool jumpInput = false;
            UpdateInputs(ref _moveInput, ref _lookInput, ref jumpInput, ref _sprintInput);

            // Move character
            MoveCharacter(moveInput, jumpInput, _sprintInput, ref _isGrounded);

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

            // Player move around (x and z values only)
            Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            Vector3 playerMovement = moveDirection.normalized * Time.deltaTime * moveSpeed;

            // Movable ground additional velocity - is there a surface we're grounded on that is currently moving? Add additional velocity from it.
            Vector3 additionalMovement = Vector3.zero;
            if (currentMovingGroundSurface != null)
            {
                // Get the current ground velocity
                currentGroundVelocity = currentMovingGroundSurface.velocity;

                // Update the last touched ground velocity
                //  Character will keep velocity of the ground they last touched while in the air.
                //  When they touch a new ground and that has velocity of 0, then there will be no additional velocity from moving ground.
                lastTouchedGroundVelocity = currentGroundVelocity;
            }
            else
            {
                // No ground = no extra velocity.
                // Seems like this is useless atm but does clear up any confusion from looking at inspector values.
                currentGroundVelocity = Vector3.zero;
            }

            // Character will add velocity of the ground they last touched (ex: while in the air).
            additionalMovement = (lastTouchedGroundVelocity * Time.deltaTime);

            //  Add in movable ground velocity, from the last moving ground surface that player touched.
            Vector3 finalHorizontalMovement = playerMovement + additionalMovement;

            #region Ground & Jumping
            // Reset player velocity while touching ground.
            if (isGrounded)
            {
                playerVelocity = Vector3.zero;
            }

            // Jump if on ground - do this once per isGrounded only
            if (jumpInput && isGrounded && isGroundedAndCanJumpAgain)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * worldGravity.y);

                // Only allow one jump per accepted jump input. Waits for character to land again before can jump again.
                StartCoroutine(WaitForCharacterToLandOnGround());
            }

            // Apply gravity if not on ground
            if (!isGrounded)
                playerVelocity += worldGravity * Time.deltaTime;
            #endregion

            // Move with gravity (y value affected only)
            //  Combined into one movement movement so CharacterController.velocity reading is accurate.
            controller.Move(finalHorizontalMovement + (playerVelocity * Time.deltaTime));
        }

        /// <summary>
        /// Manages forcing player to jump only once per accepted jump input (since jumping is processed each frame; want to prevent doubling up on jump events across first couple jump frames).
        /// Sends out jump event and land event once.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator WaitForCharacterToLandOnGround()
        {
            // Minimum time between a jump and a land. Since after a jump can still be considered grounded for first couple frames.
            const float delayBetweenCheckingJumpAndLand = 0.25f;

            isGroundedAndCanJumpAgain = false;

            // Send jump event out
            OnCharacterJump?.Invoke();

            // Mark time of jump
            float timeOfJump = Time.time;
            
            // Next isGrounded=true can be accepted at this timestamp:
            float timeOfNextIsGrounded = timeOfJump + delayBetweenCheckingJumpAndLand;
            // Wait for player to land on ground after initial jump
            while (true)
            {
                // If past timestamp, check if grounded
                if (Time.time > timeOfNextIsGrounded)
                {
                    // If grounded again after acceptable timestamp, break out
                    if (isGrounded)
                        break;
                }

                // TODO: Moving and jumping doesn't really happen in fixed update at the moment (prevent jittery camera)...
                yield return new WaitForFixedUpdate();
            }

            // Allow player to jump again
            isGroundedAndCanJumpAgain = true;

            // Send land event out
            OnCharacterLand?.Invoke();
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
            if (detachedHead)
                detachedHead.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

            // Rotate player body to match camera view rotation
            if (body)
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


        #region GroundCheck - Spherecast Debug
#if UNITY_EDITOR
        // Spherecast debug vars
        Vector3 sc_position;
        float sc_radius;
        Vector3 sc_position_end;
        private void OnDrawGizmos()
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawSphere(sc_position, sc_radius);
        }
#endif
#endregion

        /// <summary>
        /// Is the current character touching a ground surface?
        /// <para>Additionally detects and sets <see cref="currentMovingGroundSurface"/> if the surface underneath is a <see cref="MovableGroundSurface"/>.</para>
        /// </summary>
        /// <returns>True if character is touching ground (depends on CharacterController.skinWidth).</returns>
        protected bool IsGrounded()
        {
            // Ground = anything not on the player layer
            LayerMask groundCheckLayer = this.groundCheckLayer;

            // Account for the controller's skin width in raycast + a little more
            float maxDistance = controller.skinWidth + 0.02f;
            Vector3 characterFeet = transform.position;

            float additionalRadius = 0.02f;
            float additionalDistance = 0.02f;

            bool hitGround = false;
            Vector3 spherePosition = characterFeet;
            spherePosition.y = characterFeet.y + controller.radius;
            float radius = controller.radius + additionalRadius;
            float distance = controller.skinWidth + additionalDistance;
#if UNITY_EDITOR
            sc_position = spherePosition;
            sc_radius = radius;
#endif
            if (Physics.SphereCast(spherePosition, radius, Vector3.down, out RaycastHit hitInfo, distance, groundCheckLayer))
            {
                //Debug.Log("Hit: " + hitInfo.collider.gameObject + " " + LayerMask.LayerToName(hitInfo.collider.gameObject.layer), hitInfo.collider.gameObject);
                hitGround = true;

                // Is the ground that was touched have a component to mark it as movable?
                MovableGroundSurface movableGround = hitInfo.collider.gameObject.GetComponentInParent<MovableGroundSurface>();
                if (movableGround)
                    currentMovingGroundSurface = movableGround;
                /// If hit some kind of ground object, but is not marked as movable ground, then set to null (no additional velocity to account for in <see cref="MoveCharacter"/>).
                else
                {
                    currentMovingGroundSurface = null;
                    // Additional movement velocity = 0.
                    lastTouchedGroundVelocity = Vector3.zero;
                }
            }
            else
            {
                // If not grounded - move character with previous ground's velocity
                currentMovingGroundSurface = null;
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
