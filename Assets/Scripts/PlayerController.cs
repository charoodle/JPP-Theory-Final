using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MyProject.CharacterController
{
    [Header("Sensitivity & Mouse Settings")]
    [SerializeField] float lookXSens = 750f;
    [SerializeField] float lookYSens = 750f;
    [SerializeField] bool invertLookX = false;
    [SerializeField] bool invertLookY = false;

    protected override void Start()
    {
        // Hide player's mouse cursor.
        HideMouseCursor();
        base.Start();
    }

    protected override Vector2 GetMoveInput()
    {
        Vector2 moveInput = Vector2.zero;
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");
        return moveInput;
    }

    protected override Vector2 GetLookInput()
    {
        return GetMouseInput();
    }

    protected Vector2 GetMouseInput()
    {
        Vector2 lookInput = Vector2.zero;
        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
        return lookInput;
    }

    protected override bool GetJumpInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }

    protected override bool GetSprintInput()
    {
        return Input.GetKey(KeyCode.LeftShift);
    }

    protected override Vector2 ProcessLookInput(Vector2 lookInput)
    {
        // Adjust by sensitivity
        lookInput.x *= Time.deltaTime * lookXSens;
        lookInput.y *= Time.deltaTime * lookYSens;

        // Invert look if desired
        if (invertLookX)
            lookInput.x *= -1f;
        if (invertLookY)
            lookInput.y *= -1f;

        return lookInput;
    }

    protected void HideMouseCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
