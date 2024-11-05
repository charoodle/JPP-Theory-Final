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

        // Look-around
        [SerializeField] Transform characterLook_LeftRight;
        [SerializeField] Transform characterLook_UpDown;
        float lookXRotation = 0f;
        float lookYRotation = 0f;

        // Jump
        [SerializeField] float jumpHeight = 3f;
        Vector3 worldGravity = new Vector3(0f, -9.81f, 0f);
        [SerializeField] bool isGrounded;

        // Layers
        [SerializeField] LayerMask characterLayer;
        LayerMask groundCheckLayer;


        protected void Start()
        {
            // Ground = anything not the character
            groundCheckLayer = ~characterLayer;
        }

        protected void Update()
        {
            MoveCharacter();
        }

        protected void LateUpdate()
        {
            LookPlayer();
        }

        protected void MoveCharacter()
        {
            isGrounded = IsGrounded();

            // Get player move input
            moveInput = GetMoveInput();

            // Sprinting
            float moveSpeed = walkSpeed;
            if (Input.GetKey(KeyCode.LeftShift))
                moveSpeed *= 2f;

            // Move around
            Vector3 moveDirection = transform.forward * moveInput.y + transform.right * moveInput.x;
            controller.Move(moveDirection.normalized * Time.deltaTime * moveSpeed);

            if (isGrounded)
            {
                // No gravity while touching ground.
                playerVelocity = Vector3.zero;
            }

            // Jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -2.0f * worldGravity.y);
            }

            // Gravity
            if (!isGrounded)
                playerVelocity += worldGravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }

        protected void LookPlayer()
        {
            // Get player look input
            lookInput = GetMouseInput();
            // Adjust by sensitivity
            lookInput.x *= Time.deltaTime * lookXSens;
            lookInput.y *= Time.deltaTime * lookYSens;
            lookXRotation += lookInput.x;
            lookYRotation -= lookInput.y;
            // Clamp up and down rotation
            lookYRotation = Mathf.Clamp(lookYRotation, -90f, 90f);

            // Look around
            //  x horizontal movement should rotate the player model left and right around the y axis.
            //  y vertical movement should rotate the camera around the x axis.
            characterLook_UpDown.rotation = Quaternion.Euler(lookYRotation, lookXRotation, 0f);
            characterLook_LeftRight.rotation = Quaternion.Euler(0f, lookXRotation, 0f);
        }

        protected Vector2 GetMoveInput()
        {
            Vector2 moveInput = Vector2.zero;
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
            return moveInput;
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
