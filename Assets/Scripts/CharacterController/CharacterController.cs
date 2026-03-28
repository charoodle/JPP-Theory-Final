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
        protected bool _jumpInput = false;

        // Input control
        [SerializeField] protected bool _canInputMove = true;
        [SerializeField] protected bool _canInputLook = true;
        [SerializeField] protected bool _canInputSprint = true;
        [SerializeField] protected bool _canInputJump = true;

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

        /// <summary>
        /// Is the character allowed to input move?
        /// </summary>
        public bool canInputMove
        {
            get { return _canInputMove; }
            set { _canInputMove = value; }
        }

        /// <summary>
        /// Is the character allowed to input look?
        /// </summary>
        public bool canInputLook
        {
            get { return _canInputLook; }
            set { _canInputLook = value; }
        }

        /// <summary>
        /// Is the character allowed to input sprint?
        /// </summary>
        public bool canInputSprint
        {
            get { return _canInputSprint; }
            set { _canInputSprint = value; }
        }

        /// <summary>
        /// Is the character allowed to input jump?
        /// </summary>
        public bool canInputJump
        {
            get { return _canInputJump; }
            set { _canInputJump = value; }
        }


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



        /// <summary>
        /// Controls the character's rotation.
        /// </summary>
        [Header("Rotation")]
        public RotationLookAt rot;
        [SerializeField] Transform rotateBody;
        [SerializeField] Transform rotateFreedHead;

        [Header("Jump, Ground, and Gravity")]
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

            // Initialize LookRotation
            if (rot == null)
            {
                rot = GetComponent<RotationLookAt>();
                if(rot == null)
                    Debug.LogWarning("No LookRotate component found on this CharacterController!", this.gameObject);
            }

            rot.AssignBody(rotateBody);
            rot.AssignHead(rotateFreedHead);

            // Make the object retain the same rotation it has when the game is started (yaw /Y-rotation only). Because custom-controlled rotation system.
            rot.InitializeStartingRotation(transform.rotation.eulerAngles.y);
        }


        #region Character Control
        protected virtual void Update()
        {
            // If game is paused, player cannot move, or look
            if (Time.timeScale == 0)
                return;

            // Update input
            UpdateInputs(ref _moveInput, ref _lookInput, ref _jumpInput, ref _sprintInput);

            // Move character
            MoveCharacter(moveInput, _jumpInput, _sprintInput, ref _isGrounded);

            // Update rotation (values only)
            if(rot)
                UpdateLookRotation(lookInput, ref rot.yawDegrees, ref rot.pitchDegrees);
        }

        protected void LateUpdate()
        {
        }

        protected virtual void UpdateInputs(ref Vector2 moveInput, ref Vector2 lookInput, ref bool jumpInput, ref bool sprintInput)
        {
            // Movement, if that input is allowed
            moveInput = canInputMove ? GetMoveInput() : Vector2.zero;
            jumpInput = canInputJump ? GetJumpInput() : false;
            sprintInput = canInputSprint ? GetSprintInput() : false;

            // Look, if that input is allowed
            lookInput = canInputLook ? GetLookInput() : Vector2.zero;
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

            // Send land event out
            OnCharacterLand?.Invoke();

            // Allow player to jump again
            isGroundedAndCanJumpAgain = true;
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
            Gizmos.DrawSphere(sc_position_end, sc_radius);
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
            Vector3 characterFeet = transform.position;

            float additionalRadius = 0f;
            float additionalDistance = 0.02f;

            bool hitGround = false;
            Vector3 spherePosition = characterFeet;
            spherePosition.y = characterFeet.y + controller.radius;
            float radius = controller.radius + additionalRadius;
            float distance = controller.skinWidth + additionalDistance;
#if UNITY_EDITOR
            sc_position = spherePosition;
            sc_position_end = spherePosition + (Vector3.down * distance);
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
