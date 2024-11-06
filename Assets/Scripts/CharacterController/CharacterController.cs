using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a character.
/// </summary>

namespace MyProject
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField] protected UnityEngine.CharacterController controller;

        // Input
        Vector2 moveInput;
        Vector2 lookInput;

        // Movement
        [SerializeField] Vector3 playerVelocity;
        [SerializeField] private float walkSpeed = 3.0f;
        [SerializeField] private float sprintSpeedMultiplier = 2.0f;

        // Look-around
        [SerializeField] Transform characterLook_LeftRight;
        [SerializeField] Transform characterLook_UpDown;
        [SerializeField] float yawDegrees = 0f;
        [SerializeField] float pitchDegrees = 0f;
        [SerializeField] float lookXSens = 300f;
        [SerializeField] float lookYSens = 300f;

        // Jump
        [SerializeField] float jumpHeight = 3f;
        Vector3 worldGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] bool isGrounded;

        // Layers (for jumping)
        [SerializeField] LayerMask characterLayer;
        LayerMask groundCheckLayer;


        protected virtual void Start()
        {
            // Ground = anything not the character layer
            groundCheckLayer = ~characterLayer;
        }

        protected virtual void Update()
        {
            // Update input
            bool jumpInput = false;
            bool sprintInput = false;
            UpdateInputs(ref moveInput, ref lookInput, ref jumpInput, ref sprintInput);
            
            // Move character
            MoveCharacter(moveInput, jumpInput, sprintInput, ref isGrounded);

            // Update rotation (values only)
            UpdateLookRotation(lookInput, ref yawDegrees, ref pitchDegrees);
        }

        protected void LateUpdate()
        {
            // Look-around character
            CharacterLookAround(yawDegrees, pitchDegrees);
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

        protected virtual Vector2 ProcessLookInput(Vector2 lookInput)
        {
            // Get player look input
            lookInput = GetMouseInput();
            // Adjust by sensitivity
            lookInput.x *= Time.deltaTime * lookXSens;
            lookInput.y *= Time.deltaTime * lookYSens;
            return lookInput;
        }

        protected void MoveCharacter(Vector3 moveInput, bool jumpInput, bool sprintInput, ref bool isGrounded)
        {
            // Update ground check
            isGrounded = IsGrounded();

            // Sprinting - modify final move speed
            float moveSpeed = walkSpeed;
            if (sprintInput)
                moveSpeed *= sprintSpeedMultiplier;

            // Move around (x and z values only)
            Vector3 moveDirection = transform.forward * this.moveInput.y + transform.right * this.moveInput.x;
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

        protected void CharacterLookAround(float yawDegrees, float pitchDegrees)
        {
            // Rotate camera - assumes its on a separate object from character that follow's character's body
            characterLook_UpDown.rotation = Quaternion.Euler(pitchDegrees, yawDegrees, 0f);

            // Rotate player body to match camera view rotation
            characterLook_LeftRight.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
        }

        protected void UpdateLookRotation(Vector2 lookInput, ref float lookXRotation, ref float lookYRotation)
        {
            // Change x and y rotations by input
            lookXRotation += lookInput.x;
            lookYRotation -= lookInput.y;
            // Clamp up and down rotation
            lookYRotation = Mathf.Clamp(lookYRotation, -90f, 90f);
        }

        protected virtual Vector2 GetMoveInput()
        {
            Vector2 moveInput = Vector2.zero;
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
            return moveInput;
        }

        protected virtual Vector2 GetLookInput()
        {
            return GetMouseInput();
        }

        protected virtual bool GetJumpInput()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        protected virtual bool GetSprintInput()
        {
            return Input.GetKey(KeyCode.LeftShift);
        }

        protected Vector2 GetMouseInput()
        {
            Vector2 lookInput = Vector2.zero;
            lookInput.x = Input.GetAxis("Mouse X");
            lookInput.y = Input.GetAxis("Mouse Y");
            return lookInput;
        }

        protected bool IsGrounded()
        {
            // Ground = anything not on the player layer
            Vector3 playerFeet = transform.position;

            // Account for the controller's skin width in raycast
            float maxDistance = controller.skinWidth;

            // Debug draw the raycast line
            //Debug.DrawLine(playerFeet, playerFeet + (Vector3.down * maxDistance), Color.red);

            if (Physics.Raycast(playerFeet, Vector3.down, out RaycastHit hitInfo, maxDistance, groundCheckLayer))
            {
                //Debug.Log("Hit: " + hitInfo.collider.gameObject + " " + LayerMask.LayerToName(hitInfo.collider.gameObject.layer), hitInfo.collider.gameObject);
                return true;
            }

            return false;
        }
    }
}
