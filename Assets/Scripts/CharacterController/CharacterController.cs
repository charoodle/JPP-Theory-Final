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

        // Temporary - LookAt debugging
        public Transform lookAtTfm;

        protected virtual void Start()
        {
            // Ground = anything not the character layer
            groundCheckLayer = ~characterLayer;

            // Original spawn position, in case ever fall out of map
            spawnPosition = transform.position;

            // Check player from going out of bounds every couple secons
            StartCoroutine(PreventOutOfBoundsCoroutine());

            // DEBUG: Make player look at a point in space
            //LookAt()
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

        #region Look At Functions
        protected virtual void LookAt(Transform position, float rotateSpeed = 5f)
        {
            StartCoroutine(LookAtCoroutine(lookAtTfm, rotateSpeed));
        }

        protected virtual IEnumerator LookAtCastleAndThenBackToPrevious()
        {
            Transform castle = GameObject.Find("EnemyCastle").transform;
            float pitch = pitchDegrees;
            float yaw = yawDegrees;
            // Look at the castle in distance
            yield return LookAtCoroutine(castle.position);
            // Hold look there for a second
            yield return new WaitForSeconds(1f);
            // Return to previous rotation
            yield return LookAtCoroutine(pitch, yaw);
        }

        /// <summary>
        /// Make the character controller permanently look at a transform (until stopped).
        /// Uses the look rotation's pitch and yaw system to get a target pitch/yaw to smoothly rotate towards the target transform.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual IEnumerator LookAtCoroutine(Transform target, float lookSpeed, float lookTime = 0.5f, float initialLookVel = 0f)
        {
            float yawVel = initialLookVel;
            float pitchVel = initialLookVel;
            while (true)
            {
                float currentYaw = yawDegrees;
                float currentPitch = pitchDegrees;

                // Get target pitch and yaw from a world position for char to look at
                GetTargetPitchAndYawFrom(target.position, out float targetYaw, out float targetPitch);

                // Make sure the target angle has same system as this cc's yaw system.
                KeepYawBetween180(ref targetYaw);

                // Must use opposite version of yawDegrees angle if yawDegrees --> targetYaw crosses over from -180 to 180 (and vice versa).
                currentYaw = DetectIfYawPassesOver180(currentYaw, targetYaw);

                // SmoothDamp current yaw and pitch towards target yaw/pitch
                this.yawDegrees = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVel, lookTime);
                this.pitchDegrees = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVel, lookTime);

                // Yaw - Convert final angle to -180 to 180
                KeepYawBetween180(ref yawDegrees);
                // Pitch - Clamp final angle from -90 to 90
                pitchDegrees = Mathf.Clamp(pitchDegrees, maxPitchDegreesDown, maxPitchDegreesUp);

                yield return null;
            }
        }

        /// <summary>
        /// Make the character controller rotate to look at a target pitch and yaw roughly within <paramref name="lookTime"/> seconds.
        /// </summary>
        /// <param name="targetPitch">Target pitch degrees to look at.</param>
        /// <param name="targetYaw">Target yaw degrees to look at.</param>
        /// <param name="lookTime">Roughly how many seconds until is fully looking at target pitch/yaw.</param>
        /// <param name="initialLookVel">Higher number = faster initial look velocity.</param>
        /// <returns></returns>
        protected virtual IEnumerator LookAtCoroutine(float targetPitch, float targetYaw, float lookTime = 0.5f, float initialLookVel = 0f)
        {
            /*
             * Use cases:
             * 
             * Enemy looks towards castle door to march towards it
             *  yield return LookAt(castleTfm, lookSpeed)
             * 
             * Enemy looks towards player to shoot at them and locks onto them for a duration of time (and then next step makes them look at something else)
             *  yield return LookAt(playerTfm, forDuration, lookSpeed)
             */

            /*
             * Function declarations:
             * protected virtual IEnumerator LookAt(Transform tfm, float lookSpeed)
             * 
             * protected virtual IEnumerator LookAt(Vector3 worldPos, float overTime)
             * protected virtual IEnumerator LookAt(float pitch, float yaw, float overTime)
             */

            // TODO:
            // Lerp to look at the target tfm within seconds
            //  Look at target within x seconds

            // Lerp and hold lock on that target tfm for seconds
            //  Look at target with instantaneous speed for x seconds

            // Cannot have negative look time.
            if(lookTime < 0)
            {
                Debug.LogWarning("LookAtCoroutine: Look time cannot be negative.");
                yield break;
            }

            // Cannot divide by 0 in lerp function. Skips loop.
            if(lookTime == 0)
            {
                this.pitchDegrees = targetPitch;
                this.yawDegrees = targetYaw;
                yield break;
            }

            bool IsCloseEnough(float degrees, float targetDegrees)
            {
                return Mathf.Abs(targetDegrees - degrees) <= 0.1f;
            }
                
            float yawVel = initialLookVel;
            float pitchVel = initialLookVel;

            // TODO: If initialLookVel has opposite signage of yaw/pitch, then it can make it lerp the opposite way temporarily (even if no movement should happen).

            while (!IsCloseEnough(yawDegrees, targetYaw) || !IsCloseEnough(pitchDegrees, targetPitch))
            {
                float currentYaw = yawDegrees;
                float currentPitch = pitchDegrees;

                // Must use opposite version of yawDegrees angle if yawDegrees --> targetYaw crosses over from -180 to 180 (and vice versa).
                currentYaw = DetectIfYawPassesOver180(currentYaw, targetYaw);

                // Make sure the target angle has same system as this cc's yaw system.
                KeepYawBetween180(ref targetYaw);

                // SmoothDamp current yaw and pitch towards target yaw/pitch
                this.yawDegrees = Mathf.SmoothDamp(currentYaw, targetYaw, ref yawVel, lookTime);
                this.pitchDegrees = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVel, lookTime);

                // Convert to -180 to 180 for pitch system
                KeepYawBetween180(ref yawDegrees);
                // Pitch - Clamp from -90 to 90
                pitchDegrees = Mathf.Clamp(pitchDegrees, maxPitchDegreesDown, maxPitchDegreesUp);

                yield return null;
            }

            // Make sure pitch/yaw values = target.
            pitchDegrees = targetPitch;
            yawDegrees = targetYaw;
        }

        /// <summary>
        /// Makes character's head look at a world position over time.
        /// </summary>
        /// <param name="worldPos">Position to look at.</param>
        /// <param name="lookTime">Roughly how many seconds until head looks at that direction from current direction.</param>
        /// <returns></returns>
        protected virtual IEnumerator LookAtCoroutine(Vector3 worldPos, float lookTime = 0.5f, float initialLookVel = 0f)
        {
            
            GetTargetPitchAndYawFrom(worldPos, out float yaw, out float pitch);
            yield return LookAtCoroutine(pitch, yaw, lookTime, initialLookVel);
        }

        /// <summary>
        /// Converts the world position into a target yaw and pitch relative to character's head.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="yaw"></param>
        /// <param name="pitch"></param>
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

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                StopAllCoroutines();
                StartCoroutine(LookAtCastleAndThenBackToPrevious());
            }

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

            // Account for the controller's skin width in raycast
            float maxDistance = controller.skinWidth;
            Vector3 characterFeet = transform.position;

            // Debug draw the raycast line
            //Debug.DrawLine(playerFeet, playerFeet + (Vector3.down * maxDistance), Color.red);

            if (Physics.Raycast(characterFeet, Vector3.down, out RaycastHit hitInfo, maxDistance, groundCheckLayer))
            {
                //Debug.Log("Hit: " + hitInfo.collider.gameObject + " " + LayerMask.LayerToName(hitInfo.collider.gameObject.layer), hitInfo.collider.gameObject);
                return true;
            }

            return false;
        }
    }
}
