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

            // TODO: Make player look at a point in space
            LookAt(lookAtTfm);
        }

        protected IEnumerator PreventOutOfBoundsCoroutine()
        {
            float checkSeconds = 5f;
            while(true)
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

        protected virtual void LookAt(Transform position)
        {
            StartCoroutine(LookAtCoroutine(lookAtTfm));
        }

        protected IEnumerator LookAtCoroutine(Transform tfm)
        {
            float lookSpeed = 0.5f;
            while (true)
            {
                // Get target pitch and yaw from a world position for char to look at
                GetTargetPitchAndYawFrom(tfm.position, out float targetYaw, out float targetPitch);

                // Make sure the target angle has same system as this cc's yaw system.
                KeepYawBetween180(ref yawDegrees);
                KeepYawBetween180(ref targetYaw);

                // Modify in lerp
                float yawDegreesLerpAngle = yawDegrees;

                // If targetYaw has a different sign than current one, make yawDegrees into equivalent negative version
                // Since it would cause it to lerp from 179.999 to -179.999 (making char spin all the way around)
                if (Mathf.Sign(targetYaw) != Mathf.Sign(yawDegrees))
                {
                    // Yaw for this character controller follows euler angles and goes from -180 to 180 (where 0 = z-forward, and 180/-180 = z-back).
                    // Breaking point between -180 and 180; must use opposite angles if crossing over the 180 mark from either direction.
                    // Crossing over the 0 mark is perfectly fine.
                    if(Mathf.Abs(yawDegrees) + Mathf.Abs(targetYaw) > 180f)
                    {
                        // Substitute yawDegrees with its opposite positive/negative equivalent.
                        //yawDegreesLerpAngle = (360f - yawDegreesLerpAngle) * (-1f * Mathf.Sign(yawDegreesLerpAngle));
                        if (yawDegreesLerpAngle < 0f)
                        {
                            yawDegreesLerpAngle = 360f + yawDegreesLerpAngle;
                        }
                        else if (yawDegreesLerpAngle >= 0)
                        {
                            yawDegreesLerpAngle = (360f - yawDegreesLerpAngle) * -1f;
                        }
                    }
                }

                // Yaw - Lerp current yaw and pitch towards target yaw/pitch
                this.yawDegrees = Mathf.Lerp(yawDegreesLerpAngle, targetYaw, Time.deltaTime * lookSpeed);
                // Convert to -180 to 180 for pitch system
                KeepYawBetween180(ref yawDegrees);

                // Pitch - Clamp from -90 to 90
                this.pitchDegrees = Mathf.Lerp(pitchDegrees, targetPitch, Time.deltaTime * lookSpeed);
                pitchDegrees = Mathf.Clamp(pitchDegrees, maxPitchDegreesDown, maxPitchDegreesUp);

                yield return null;
            }
        }

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

            // DEBUG - ignore pitch, focus only on yaw
            pitch = 0f;

            // If look object goes above head object, euler X will wrap around from 1* to 360*.
            if (quat.eulerAngles.x > 180f)
                pitch = quat.eulerAngles.x - 360f;
        }

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
