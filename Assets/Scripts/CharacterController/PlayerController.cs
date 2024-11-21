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
    public const float interactRange = 5f;

    [SerializeField] protected ProjectileLauncher[] weapons;
    protected ProjectileLauncher currentWeapon;

    [SerializeField] protected bool _canInputMove = true;
    public bool canInputMove
    {
        get { return _canInputMove; }
        set { _canInputMove = value; }
    }
    [SerializeField] protected bool _canInputLook = true;
    public bool canInputLook
    {
        get { return _canInputLook; }
        set { _canInputLook = value; }
    }

    protected enum Weapon
    {
        PISTOL = 1,
        RL = 2,
        DISARMED = 3
    }

    protected override void Start()
    {
        // Hide player's mouse cursor.
        HideMouseCursor();
        base.Start();
    }

    protected override void Update()
    {
        // Additional weapon switch input
        Weapon switchToWeapon = 0; 
        bool switchWeapon = GetWeaponSwitchInput(ref switchToWeapon);

        // Switch Weapon
        if(switchWeapon)
            SwitchWeapon(switchToWeapon);

        // Player interaction only if interact text is shown on screen
        bool interactButton = GetInteractButtonInput();
        if (interactButton && Interactable.showInteractTextOnScreen)
        {
            bool targetFound = false;
            GameObject target = ProjectileLauncher.GetPlayersCenterCameraTarget_GameObject(ref targetFound, interactRange);
            if(targetFound)
            {
                Interactable interactable = target.GetComponent<Interactable>();
                if(interactable)
                    interactable.InteractWith();
            }
        }

        base.Update();
    }

    protected virtual void SwitchWeapon(Weapon weaponToSwitchTo)
    {
        // Unequip current weapon
        UnequipCurrentWeaponGameObject();

        // Equip the correct weapon
        if(weaponToSwitchTo == Weapon.PISTOL) {
            currentWeapon = EquipWeaponGameObject("Pistol");
        }
        else if(weaponToSwitchTo == Weapon.RL) {
            currentWeapon = EquipWeaponGameObject("RocketLauncher");
        }
        else if(weaponToSwitchTo == Weapon.DISARMED) {
            UnequipCurrentWeaponGameObject();
        }
        else {
            Debug.LogError("Weapon not supported: " + weaponToSwitchTo);
        }
    }

    protected virtual ProjectileLauncher EquipWeaponGameObject(string weaponName)
    {
        foreach (ProjectileLauncher weapon in weapons)
        {
            if (weapon.name == weaponName)
            {
                weapon.EquipWeapon(true);
                return weapon;
            }
        }

        // No weapon found to equip with string name
        Debug.LogError("Cannot find and equip weapon with name: " + weaponName);
        return null;
    }

    protected virtual void UnequipCurrentWeaponGameObject()
    {
        if(currentWeapon)
        {
            currentWeapon.EquipWeapon(false);
            currentWeapon = null;
        }
    }

    protected virtual bool GetWeaponSwitchInput(ref Weapon weaponKey)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weaponKey = Weapon.PISTOL;
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weaponKey = Weapon.RL;
            return true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weaponKey = Weapon.DISARMED;
            return true;
        }

        return false;
    }

    protected override Vector2 GetMoveInput()
    {
        Vector2 moveInput = Vector2.zero;
        if(canInputMove)
        {
            moveInput.x = Input.GetAxis("Horizontal");
            moveInput.y = Input.GetAxis("Vertical");
        }
        return moveInput;
    }

    protected override Vector2 GetLookInput()
    {
        return GetMouseInput();
    }

    protected Vector2 GetMouseInput()
    {
        Vector2 lookInput = Vector2.zero;
        if (canInputLook)
        {
            lookInput.x = Input.GetAxis("Mouse X");
            lookInput.y = Input.GetAxis("Mouse Y");
        }
        return lookInput;
    }

    protected override bool GetJumpInput()
    {
        if (!canInputMove)
            return false;

        return Input.GetKey(KeyCode.Space);
    }

    protected override bool GetSprintInput()
    {
        if (!canInputMove)
            return false;

        return Input.GetKey(KeyCode.LeftShift);
    }

    protected virtual bool GetInteractButtonInput()
    {
        return Input.GetKeyDown(KeyCode.E);
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
