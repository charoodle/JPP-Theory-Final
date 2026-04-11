using System.Collections;
using UnityEngine;


namespace MyProject
{
    /// <summary>
    /// Controls a character.
    /// Must be a child of some parent GameObject. I think for parenting to moving objects.
    /// 
    /// TODO: Refactor so can port into Bulletpain
    ///     - [ ] Refactor each section into separate scripts?
    ///         [ ] Input
    ///         [ ] Move Object
    ///         [ ] Rotate Object
    ///     - [ ] Add documentation where needed + make consistent variable naming
    ///     
    /// Issues:
    ///     - Character jumping is slightly higher than <see cref="_jumpHeight"/> on different machines?
    ///         - Works fine on mine though...
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
            _groundCheckLayer = ~_characterLayer;

            // Original spawn position, in case ever fall out of map
            _spawnPosition = transform.position;

            // Checks player from going out of bounds every couple seconds
            StartCoroutine(PreventOutOfBoundsCoroutine());

            // Get one layer above the root game object as the original root. Assumes (!!!) char controller is only one layer deep (?).
            // TODO: This seems bad...
            if (_rootGameObject)
            {
                _originalRoot = _rootGameObject.transform.parent;
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

            /// TODO:
            ///     Clean up function call order for MoveCharacter, UpdateGravity, etc. Out of order/looks too messy.

            // Check for ground before moving character
            Vector3 movingSurfaceVelocity = GetCurrentGroundVelocity();
            
            // Is the character controller touching a ground surface?
            _isGrounded = CheckIsGrounded(charController);

            // What is the gravity force?
            UpdateCurrentGravityVelocity(_isGrounded, _input.jumpInput, ref _currentGravityVelocity, _jumpHeight, _worldGravity);
            Vector3 gravityVelocity = _currentGravityVelocity;

            // Move character
            MoveCharacter(_input.moveInput, _input.sprintInput, movingSurfaceVelocity, gravityVelocity, this.controller);

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
        protected virtual float _WalkSpeed
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

        /// <summary>
        /// Without deltaTime.
        /// </summary>
        [Header("Movement")]
        [SerializeField] protected Vector3 _movementVector;
        /// <summary>
        /// TODO: For some reason it gets used to add gravity in the MoveCharacter function. Take this member var out of that function.
        /// TODO: This only works with gravity. Does not update X/Z velocity.
        /// </summary>
        [SerializeField] protected Vector3 _movementVectorDeltaTime;

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
        [SerializeField] private Transform _rootGameObject;
        /// <summary>
        /// Original root object on game start. Used to parent/unparent this object back and forth from to this transform (ex: ground checks).
        /// </summary>
        protected Transform _originalRoot;

        // Movement with a parent
        /// <summary>
        /// Is the character controller currently grounded on something that can move? May need to supply additional movement to the CharacterController.Move() function.
        /// </summary>
        [SerializeField] protected MovableGroundSurface _currentMovingGroundSurface;
        [SerializeField] protected Vector3 _currentGroundVelocity;
        [SerializeField] protected Vector3 _lastTouchedGroundVelocity;

        /// <summary>
        /// How high the character jumps (in default Unity units) before gravity pulls them down.
        /// </summary>
        [Header("Jump/Gravity")]
        [SerializeField] protected float _jumpHeight = 3f;

        /// <summary>
        /// Gravity for this character.
        /// </summary>
        private Vector3 _worldGravity = new Vector3(0f, -9.81f, 0f);

        /// <summary>
        /// Current gravity value acting on player this frame. Adds more and more gets added until player touches ground again. Gets modified by <see cref="UpdateCurrentGravityVelocity(bool, bool, ref Vector3, float, Vector3)"/>
        /// </summary>
        protected Vector3 _currentGravityVelocity = Vector3.zero;

        /// <summary>
        /// Spawn position
        /// </summary>
        protected Vector3 _spawnPosition;

        /// <summary>
        /// Flag to manage allowing one jump command per accepted character jump inpu. This should initially be true on game start to let the character jump.
        /// <para>Triggered by <see cref="WaitForCharacterToLandOnGround"/>, and is used in the jump section of <see cref="MoveCharacter(Vector3, bool, Vector3, Vector3)"/></para>
        /// </summary>
        protected bool _isGroundedAndCanJumpAgain = true;

        // Layers (for jumping)
        [SerializeField] private LayerMask _characterLayer;
        private LayerMask _groundCheckLayer;

        /// <summary>
        /// If a character falls below this y-value, they are considered out of bounds and will be reset to their original spawn position.
        /// </summary>
        protected const float Y_AXIS_OUTOFBOUNDS = -30f;

        /// <summary>
        /// 
        /// TODO:
        ///     Refactor
        ///         What does moving the character need to do?
        ///             Use given input to determine how to move the character this frame
        ///             
        /// 
        /// </summary>
        /// <param name="moveInput"></param>
        /// <param name="sprintInput"></param>
        /// <param name="extraVelocityForces">Extra velocity that is added onto final character movement vector (ex: if the ground underneath can move). Gets multiplied by Time.deltaTime after passing in.</param>
        /// <param name="gravityForces">How much gravity is added to pull player down this frame.</param>
        protected void MoveCharacter(Vector3 moveInput, bool sprintInput, Vector3 extraVelocityForces, Vector3 gravityForces, UnityEngine.CharacterController controller)
        {
            // Does input tell the character to sprint? - Multiply current base move speed (don't really need this each frame but w/e for now)
            float moveSpeed = _WalkSpeed;
            if (sprintInput)
                moveSpeed *= _sprintSpeedMultiplier;

            // Does input tell the character to move? (Horizontal only - x/z)
            //  Get world direction based on (character dir * input dir)
            Vector3 moveDirection = ((transform.forward * moveInput.y) + (transform.right * moveInput.x)).normalized;
            //  Account for character's move speed stat.
            Vector3 movement = moveDirection * moveSpeed;

            // --------------------------------------------------

            // Add in any external forces (ex: relative velocity from moving surfaces)
            Vector3 finalHorizontalMovement = movement + extraVelocityForces;

            // Move with gravity (y value affected only)
            //  Combined into one movement so CharacterController.velocity reading is accurate.
            Vector3 finalMovementVector = finalHorizontalMovement + gravityForces;

            // Show movement vector in inspector
            _movementVector = finalMovementVector;

            // Time.deltaTime to account for game frame rate
            finalMovementVector *= Time.deltaTime;

            // Show movement vector in inspector (without delta time)
            _movementVectorDeltaTime = finalMovementVector;

            // Move the character
            controller.Move(finalMovementVector);
        }

        /// <summary>
        /// Calculate amount of gravity force for the character this frame.
        /// </summary>
        /// <returns></returns>
        /// <param name="currentAccumulatedGravity">How much gravity is acting on this player. Accumulates over time as character keeps falling. Does not apply Time.delta time.</param>
        protected void UpdateCurrentGravityVelocity(bool isGrounded, bool jumpInput, ref Vector3 currentAccumulatedGravity, float jumpHeight, Vector3 worldGravity)
        {
            // Reset character's velocity while touching ground.
            if (isGrounded)
            {
                currentAccumulatedGravity = Vector3.zero;
            }

            // Jump if on ground - do this once per isGrounded only
            if (jumpInput && isGrounded && _isGroundedAndCanJumpAgain)
            {
                // Impulse jump. If jump, go against gravity for one frame. No time.deltaTime needed?

                // (?) -2 is some kind of constant, so if set jumpHeight to 1, then character actually jumps up 1 unity unit.
                currentAccumulatedGravity.y += Mathf.Sqrt(jumpHeight * -2.0f * worldGravity.y);

                // Only allow one jump per accepted jump input. Waits for character to land again before can jump again.
                StartCoroutine(WaitForCharacterToLandOnGround());
            }

            // Apply gravity if not on ground
            //  TODO: I have no idea why this works. Time.deltaTime is multiplied twice. Character.Move multiplies the second time. But it seems like it works with this here...?
            if (!isGrounded)
                currentAccumulatedGravity += (worldGravity * Time.deltaTime);
        }

        /// <summary>
        /// Checks for relative velocity from the current surface underneath the character.
        /// </summary>
        /// <returns>The velocity to add to the character controller's current movement velocity. (Does NOT account for Time.deltaTime).</returns>
        protected Vector3 GetCurrentGroundVelocity()
        {
            Vector3 groundVelocity = Vector3.zero;

            // Check if below surface is moving ground.

            // Movable ground additional velocity - is there a surface we're grounded on that is currently moving? Add additional velocity from it.
            if (_currentMovingGroundSurface != null)
            {
                // Cache current ground velocity (for inspector?)
                _currentGroundVelocity = _currentMovingGroundSurface.velocity;

                // Update the last touched ground velocity
                //  Character will keep velocity of the ground they last touched while in the air.
                //  When they touch a new ground and that has velocity of 0, then there will be no additional velocity from moving ground.
                _lastTouchedGroundVelocity = _currentGroundVelocity;
            }
            else
            {
                // No ground = no extra velocity to add. Unless jumped off from a moving ground surface.

                // Seems like this is useless atm but it does clear up any confusion from looking at inspector values.
                _currentGroundVelocity = Vector3.zero;
            }

            // Character will add velocity of the ground they last touched (ex: while in the air).
            //  Does not account for Time.delta time.
            groundVelocity = (_lastTouchedGroundVelocity);

            return groundVelocity;
        }

        /// <summary>
        /// Is this character touching a ground surface?
        /// <para>Additionally detects and sets <see cref="_currentMovingGroundSurface"/> if the surface underneath is a <see cref="MovableGroundSurface"/>.</para>
        /// </summary>
        /// <returns>True if character is touching ground (depends on CharacterController.skinWidth).</returns>
        protected bool CheckIsGrounded(UnityEngine.CharacterController charController)
        {
            // Ground = anything not on the player layer
            LayerMask groundCheckLayer = this._groundCheckLayer;

            // Account for the controller's skin width in raycast + a little more
            Vector3 characterFeet = transform.position;

            float additionalRadius = 0f;
            float additionalDistance = 0.02f;

            bool hitGround = false;
            Vector3 spherePosition = characterFeet;
            spherePosition.y = characterFeet.y + charController.radius;
            float radius = charController.radius + additionalRadius;
            float distance = charController.skinWidth + additionalDistance;
#if UNITY_EDITOR
            sc_position = spherePosition;
            sc_position_end = spherePosition + (Vector3.down * distance);
            sc_radius = radius;
#endif

            // Raycast down to find a moving surface
            if (Physics.SphereCast(spherePosition, radius, Vector3.down, out RaycastHit hitInfo, distance, groundCheckLayer))
            {
                //Debug.Log("Hit: " + hitInfo.collider.gameObject + " " + LayerMask.LayerToName(hitInfo.collider.gameObject.layer), hitInfo.collider.gameObject);
                hitGround = true;

                // Is the ground that was touched have a component to mark it as movable?
                MovableGroundSurface movableGround = hitInfo.collider.gameObject.GetComponentInParent<MovableGroundSurface>();
                if (movableGround)
                    _currentMovingGroundSurface = movableGround;
                /// If hit some kind of ground object, but is not marked as movable ground, then set to null (no additional velocity to account for in <see cref="MoveCharacter"/>).
                else
                {
                    _currentMovingGroundSurface = null;
                    // Additional movement velocity = 0.
                    _lastTouchedGroundVelocity = Vector3.zero;
                }
            }
            else
            {
                // If not grounded - move character with previous ground's velocity
                _currentMovingGroundSurface = null;
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

            _isGroundedAndCanJumpAgain = false;

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
            _isGroundedAndCanJumpAgain = true;
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
                    transform.position = _spawnPosition;
                    controller.enabled = true;
                }

                yield return new WaitForSeconds(checkSeconds);
            }
        }
        #endregion


        #region Rotate Object
        /// <summary>
        /// Controls the character's rotation.
        /// TODO: Make into a property instead.
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
