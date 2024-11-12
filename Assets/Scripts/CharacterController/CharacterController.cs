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

            // TODO: Make player look directly to front
            LookAt(transform.position + transform.forward);
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

        protected virtual void LookAt(Vector3 worldPosition)
        {
            Vector3 head = rotateFreedHead.transform.position;
            Vector3 playerForward = rotateFreedHead.transform.forward;
            Vector3 playerForwardYawOnly = new Vector3(playerForward.x, 0f, playerForward.z);

            // Yaw only (x and z only)
            Vector3 worldPositionYawOnly = new Vector3(worldPosition.x, 0f, worldPosition.z);
            Vector3 playerPosYawOnly = new Vector3(head.x, 0f, head.z);
            Vector3 playerToPosition = worldPositionYawOnly - playerPosYawOnly;
            float yawAngle = Vector3.Angle(playerForwardYawOnly, playerToPosition);

            // Pitch only (y only) - get player y pos and the world position on same plane so can calculate pitch
            Vector3 playerForward_SamePlane = new Vector3(worldPosition.x, playerForward.y, worldPosition.z);
            float pitchAngle = Vector3.Angle(playerForward_SamePlane, worldPosition);

            // Red = player forward viewing vector. Green = target vector.
            Debug.DrawRay(head, playerForward_SamePlane, Color.red, 100f);
            Debug.DrawRay(head, worldPosition, Color.green, 100f);

            Debug.Break();
            
            Debug.Log("[Expected: 0*] - Yaw Angle: " + yawAngle);
            Debug.Log("[Expected: 45*] - Pitch Angle: " + pitchAngle);
        }

        protected IEnumerator LookAtCoroutine()
        {
            // Rotate character controller look rotation to look towards a worldPosition
            yield break;
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
            //UpdateLookRotation(lookInput, ref yawDegrees, ref pitchDegrees);
        }

        protected void LateUpdate()
        {
            // Look-around character
            //CharacterLookAround(yawDegrees, pitchDegrees, rotateFreedHead, rotateBody);
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
        protected void CharacterLookAround(float yawDegrees, float pitchDegrees, Transform detachedHead, Transform body)
        {
            // Rotate camera - assumes its on a separate object from character that follow's character's body
            detachedHead.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

            // Rotate player body to match camera view rotation
            body.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }

        protected void UpdateLookRotation(Vector2 lookInput, ref float lookXRotation, ref float lookYRotation)
        {
            // Change x and y rotations by input
            lookXRotation += lookInput.x;
            lookYRotation -= lookInput.y;
            // Clamp up and down rotation
            lookYRotation = Mathf.Clamp(lookYRotation, maxPitchDegreesDown, maxPitchDegreesUp);
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
