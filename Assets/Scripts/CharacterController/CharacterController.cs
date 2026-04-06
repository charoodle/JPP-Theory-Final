using System.Collections;
using UnityEngine;


namespace MyProject
{
    /// <summary>
    /// Controls a character.
    /// Must be a child of some parent GameObject. I think for parenting to moving objects.
    /// 
    /// TODO: Refactor so can port into Bulletpain
    ///     - [ ] Move all functions, events, etc. into clearly defined sections:
    ///         [x] 1. Take input
    ///             - [x] After refactoring the inputs into a class, test the AnnouncerTutorial to make sure it works with new moveInput.
    ///         [ ] 2. Move Object
    ///         [x] 3. Rotate Object
    ///     - [ ] Refactor each section into separate scripts?
    ///         [ ] Input
    ///         [ ] Move Object
    ///         [ ] Rotate Object
    ///     - [ ] Add documentation where needed + make consistent variable naming
    ///     
    ///     
    /// </summary>
    public abstract class CharacterController : MonoBehaviour
    {
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


        #region LifeCycle Functions
        protected virtual void Start()
        {
            // Ground = anything not the character layer
            groundCheckLayer = ~characterLayer;

            // Original spawn position, in case ever fall out of map
            spawnPosition = transform.position;

            // Checks player from going out of bounds every couple seconds
            StartCoroutine(PreventOutOfBoundsCoroutine());

            // Get one layer above the root game object as the original root. Assumes (!!!) char controller is only one layer deep (?).
            // TODO: This seems bad...
            if (rootGameObject)
            {
                originalRoot = rootGameObject.transform.parent;
            }

            // Initialize LookRotation
            if (rot == null)
            {
                rot = GetComponent<RotationLookAt>();
                if (rot == null)
                    Debug.LogWarning("No LookRotate component found on this CharacterController!", this.gameObject);
            }

            rot.AssignBody(rotateBody);
            rot.AssignHead(rotateFreedHead);

            // Make the object retain the same rotation it has when the game is started (yaw /Y-rotation only). Because custom-controlled rotation system.
            rot.InitializeStartingRotation(transform.rotation.eulerAngles.y);

            // Initialize input accessors/properties where to read character's input from
            input = new CharacterControllerInputsProperties(_input);
        }


        /// <summary>
        /// Update input, and move/rotate the character based on the given input this frame.
        /// </summary>
        protected virtual void Update()
        {
            // If game is paused, player cannot move, or look
            if (Time.timeScale == 0)
                return;

            // Update input
            UpdateControllerInputs(_input);

            // Move character
            MoveCharacter(_input.moveInput, _input.jumpInput, _input.sprintInput, ref _isGrounded);

            // Update rotation (values only)
            if (rot)
                UpdateLookRotation(_input.lookInput, ref rot.yawDegrees, ref rot.pitchDegrees);
        }
        #endregion


        #region Take Input
        /// <summary>
        /// Represents inputs that a <see cref="MyProject.CharacterController"/> will use to move/rotate itself this frame.
        /// <para>
        /// Meant to use a "protected" instance of this class, and use in conjunction a "public" <see cref="CharacterControllerInputsProperties"/> for controlled getters/setters to specific fields.
        /// </para>
        /// </summary>
        [System.Serializable]
        public class CharacterControllerInputs
        {
            /// <summary>
            /// Current move input being fed in.
            /// </summary>
            [Header("Current Input")]
            public Vector2 moveInput;

            /// <summary>
            /// Current look input being fed in.
            /// </summary>
            public Vector2 lookInput;

            /// <summary>
            /// Current jump input being fed in.
            /// </summary>
            public bool jumpInput;

            /// <summary>
            /// Current sprint input being fed in.
            /// </summary>
            public bool sprintInput;

            /// <summary>
            /// Allows/disallows move input.
            /// </summary>
            [Header("Settings")]
            public bool canInputMove = true;

            /// <summary>
            /// Allows/disallows look input.
            /// </summary>
            public bool canInputLook = true;

            /// <summary>
            /// Allows/disallows jump input.
            /// </summary>
            public bool canInputJump = true;

            /// <summary>
            /// Allows/disallows sprint input.
            /// </summary>
            public bool canInputSprint = true;
        }

        /// <summary>
        /// Helper class. Allows/disallows public access to appropriate properties of the <see cref="CharacterControllerInputs"/> class.
        /// Weird workaround but (maybe) it will work. Untested atm.
        /// Works in conjuntion with a (preferably) protected variable of <see cref="CharacterControllerInputs"/>.
        /// </summary>
        public class CharacterControllerInputsProperties
        {
            /// <summary>
            /// The inputs this class is directly accessing.
            /// </summary>
            protected CharacterControllerInputs input;


            /// <param name="input">The input to directly read from.</param>
            public CharacterControllerInputsProperties(CharacterControllerInputs input)
            {
                this.input = input;
            }


            #region Current Input
            /// <inheritdoc cref="CharacterControllerInputs.moveInput"/>
            public Vector2 moveInput
            {
                get { return input.moveInput; }
            }

            /// <inheritdoc cref="CharacterControllerInputs.lookInput"/>
            public Vector2 lookInput
            {
                get { return input.lookInput; }
            }
            #endregion


            #region Settings
            /// <inheritdoc cref="CharacterControllerInputs.canInputMove"/>
            public bool canInputMove
            {
                get { return input.canInputMove; }
                set { input.canInputMove = value; }
            }

            /// <inheritdoc cref="CharacterControllerInputs.canInputLook"/>
            public bool canInputLook
            {
                get { return input.canInputLook; }
                set { input.canInputLook = value; }
            }

            /// <inheritdoc cref="CharacterControllerInputs.canInputSprint"/>
            public bool canInputSprint
            {
                get { return input.canInputSprint; }
                set { input.canInputSprint = value; }
            }

            /// <inheritdoc cref="CharacterControllerInputs.canInputJump"/>
            public bool canInputJump
            {
                get { return input.canInputJump; }
                set { input.canInputJump = value; }
            }
            #endregion
        }

        /// <summary>
        /// The current input that gets updated each frame, which controls this character.
        /// </summary>
        [SerializeField] protected CharacterControllerInputs _input = new CharacterControllerInputs();
        public CharacterControllerInputsProperties input;

        /// <summary>
        /// Update all inputs for this frame.
        /// </summary>
        /// <param name="input"></param>
        protected virtual void UpdateControllerInputs(CharacterControllerInputs input)
        {
            // Movement, if that input is allowed
            input.moveInput = input.canInputMove ? GetMoveInput() : Vector2.zero;
            input.jumpInput = input.canInputJump ? GetJumpInput() : false;
            input.sprintInput= input.canInputSprint ? GetSprintInput() : false;

            // Look, if that input is allowed
            input.lookInput = input.canInputLook ? GetLookInput() : Vector2.zero;
            // Postprocess look input if needed
            input.lookInput = PostProcessLookInput(input.lookInput);
        }


        /// <summary>
        /// Get move input. X for horizontal character movement. Y for forward/backward movement.
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 GetMoveInput();

        /// <summary>
        /// Get look input. X for yaw rotation (left/right). Y for pitch rotation (up/down).
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 GetLookInput();

        /// <returns>True if character wants to jump this frame. False otherwise.</returns>
        protected abstract bool GetJumpInput();

        /// <returns>True if character wants to sprint this frame. False otherwise.</returns>
        protected abstract bool GetSprintInput();

        /// <summary>
        /// Process look input here after it's grabbed from some source. (ex: adjust by mouse sensitivity, invert directions, ...)
        /// </summary>
        /// <param name="lookInput"></param>
        /// <returns></returns>
        protected abstract Vector2 PostProcessLookInput(Vector2 lookInput);
        #endregion


        #region Move Object (Forward/Sideways)
        /// <inheritdoc cref="_isGrounded"/>
        public bool IsGrounded
        {
            get { return _isGrounded; }
            protected set { _isGrounded = value; }
        }

        /// <summary>
        /// Is the character holding the sprint button while moving in a direction?
        /// </summary>
        public bool IsSprinting { get { return _input.sprintInput && _input.moveInput.magnitude > 0; } }

        /// <inheritdoc cref="_walkSpeed"/>
        protected virtual float WalkSpeed
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

        [Header("Movement")]
        /// <summary>
        /// TODO: This only works with gravity. Does not update X/Z velocity.
        /// </summary>
        [SerializeField] Vector3 _characterVelocity;

        /// <summary>
        /// How fast the character walks at.
        /// </summary>
        [SerializeField] protected float _walkSpeed = 3.0f;

        /// <summary>
        /// How much faster the character sprints at. Depends on <see cref="_walkSpeed"/>.
        /// </summary>
        [SerializeField] protected float _sprintSpeedMultiplier = 2.0f;

        /// <summary>
        /// Is this character touching a ground surface?
        /// </summary>
        [SerializeField] protected bool _isGrounded;

        /// <summary>
        /// Is there a higher root object to this character controller?
        /// Will be parented/unparented when they walk on surfaces.
        /// </summary>
        [Header("Movement (Relative Velocity)")]
        [SerializeField] Transform rootGameObject;
        /// <summary>
        /// Original root object on game start. Used to parent/unparent this object back and forth from to this transform (ex: ground checks).
        /// </summary>
        protected Transform originalRoot;

        // Movement with a parent
        /// <summary>
        /// Is the character controller currently grounded on something that can move? May need to supply additional movement to the CharacterController.Move() function.
        /// </summary>
        [SerializeField] MovableGroundSurface currentMovingGroundSurface;
        [SerializeField] Vector3 currentGroundVelocity;
        [SerializeField] Vector3 lastTouchedGroundVelocity;

        /// <summary>
        /// How high the character jumps (in default Unity units) before gravity pulls them down.
        /// </summary>
        [Header("Jump/Gravity")]
        [SerializeField] float _jumpHeight = 3f;

        /// <summary>
        /// Gravity for this character.
        /// </summary>
        Vector3 _worldGravity = new Vector3(0f, -9.81f, 0f);

        /// <summary>
        /// Spawn position
        /// </summary>
        Vector3 spawnPosition;

        /// <summary>
        /// Flag to manage allowing one jump command per accepted character jump inpu. This should initially be true on game start to let the character jump.
        /// <para>Triggered by <see cref="WaitForCharacterToLandOnGround"/>, and is used in the jump section of <see cref="MoveCharacter(Vector3, bool, bool, ref bool)"/></para>
        /// </summary>
        protected bool isGroundedAndCanJumpAgain = true;

        // Layers (for jumping)
        [SerializeField] LayerMask characterLayer;
        LayerMask groundCheckLayer;

        /// <summary>
        /// If a character falls below this y-value, they are considered out of bounds and will be reset to their original spawn position.
        /// </summary>
        protected const float Y_AXIS_OUTOFBOUNDS = -30f;

        protected void MoveCharacter(Vector3 moveInput, bool jumpInput, bool sprintInput, ref bool isGrounded)
        {
            // Update ground check
            isGrounded = CheckIsGrounded();

            // --------------------------------------------------

            // Does input tell the character to sprint? - Multiply current base move speed (don't really need this each frame but w/e for now)
            float moveSpeed = WalkSpeed;
            if (sprintInput)
                moveSpeed *= _sprintSpeedMultiplier;

            // Does input tell the character to move? (Horizontal only - x/z)
            //  Get world direction based on (character dir * input dir)
            Vector3 moveDirection = (transform.forward * moveInput.y) + (transform.right * moveInput.x);
            //  Normalize movement direction. Account for game frame rate.
            Vector3 playerMovement = moveDirection.normalized * Time.deltaTime;
            //  Account for character's move speed stat.
            playerMovement *= moveSpeed;

            // --------------------------------------------------

            // Check if below surface is moving ground.
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

            // --------------------------------------------------

            //  Add in movable ground velocity, from the last moving ground surface that player touched.
            Vector3 finalHorizontalMovement = playerMovement + additionalMovement;

            #region Ground & Jumping
            // Reset player velocity while touching ground.
            if (isGrounded)
            {
                _characterVelocity = Vector3.zero;
            }

            // Jump if on ground - do this once per isGrounded only
            if (jumpInput && isGrounded && isGroundedAndCanJumpAgain)
            {
                _characterVelocity.y += Mathf.Sqrt(_jumpHeight * -2.0f * _worldGravity.y);

                // Only allow one jump per accepted jump input. Waits for character to land again before can jump again.
                StartCoroutine(WaitForCharacterToLandOnGround());
            }

            // Apply gravity if not on ground
            if (!isGrounded)
                _characterVelocity += _worldGravity * Time.deltaTime;
            #endregion

            // Move with gravity (y value affected only)
            //  Combined into one movement so CharacterController.velocity reading is accurate.
            controller.Move(finalHorizontalMovement + (_characterVelocity * Time.deltaTime));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected Vector3 UpdateCurrentGroundVelocity()
        {
            return Vector3.zero;
        }

        /// <summary>
        /// Is the current character touching a ground surface?
        /// <para>Additionally detects and sets <see cref="currentMovingGroundSurface"/> if the surface underneath is a <see cref="MovableGroundSurface"/>.</para>
        /// </summary>
        /// <returns>True if character is touching ground (depends on CharacterController.skinWidth).</returns>
        protected bool CheckIsGrounded()
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
                    if (IsGrounded)
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
        
        /// <summary>
        /// Respawns the character at its original instantiation position if it goes out of bounds.
        /// </summary>
        /// <returns></returns>
        protected IEnumerator PreventOutOfBoundsCoroutine()
        {
            float checkSeconds = 5f;
            while (true)
            {
                if (transform.position.y < Y_AXIS_OUTOFBOUNDS)
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


        #region Rotate Object
        /// <summary>
        /// Controls the character's rotation.
        /// </summary>
        [Header("Rotation")]
        public RotationLookAt rot;

        /// <summary>
        /// The body of the character that rotates around its y-axis to match a pitch/yaw degrees value.
        /// </summary>
        [SerializeField] Transform rotateBody;

        /// <summary>
        /// The head of the character that will rotate around its x and y-axis to match a pitch/yaw degrees value.
        /// </summary>
        [SerializeField] Transform rotateFreedHead;

        /// <summary>
        /// The look at head of the character. Useful for an aim target during dialogue.
        /// </summary>
        public Transform head
        {
            get { return rotateFreedHead; }
        }

        /// <summary>
        /// Update the look rotation degrees by some input.
        /// </summary>
        /// <param name="lookInput"></param>
        /// <param name="lookXRotation"></param>
        /// <param name="lookYRotation"></param>
        protected void UpdateLookRotation(Vector2 lookInput, ref float lookXRotation, ref float lookYRotation)
        {
            // Change x and y rotations by input
            lookXRotation += lookInput.x;
            lookYRotation -= lookInput.y;
        }
        #endregion


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


        #region Debug
        #region GroundCheck - Spherecast Debug
#if UNITY_EDITOR
        // Spherecast debug vars
        Vector3 sc_position;
        float sc_radius;
        Vector3 sc_position_end;
        private void OnDrawGizmos()
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawSphere(sc_position, sc_radius);
            Gizmos.DrawSphere(sc_position_end, sc_radius);
        }
#endif
        #endregion
        #endregion
    }
}
