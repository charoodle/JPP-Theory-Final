using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : Menu
{
    [SerializeField] protected OptionsMenu optionsMenu;

    /// <summary>
    /// Should pause game when it appears.
    /// </summary>
    /// <param name="pauseGame"></param>
    public override void Appear(bool pauseGame = false)
    {
        base.Appear(pauseGame: true);
    }

    /// <summary>
    /// Should unpause game when it disappears.
    /// </summary>
    /// <param name="unpauseGame"></param>
    /// <returns></returns>
    public override Menu Disappear(bool unpauseGame = false)
    {
        return base.Disappear(unpauseGame: true);
    }

    /// <summary>
    /// Enter into the options menu. Used for UI buttons.
    /// </summary>
    /// <param name="enter">True if entering into a sub menu. False if exiting a sub menu.</param>
    public void EnterOptionsMenu()
    {
        // Keep it on in hierarchy?
        // Turn off current menu's contents --> set active false
        contents.SetActive(false);

        // Turn background image off
        menuBackground.enabled = false;

        // Turn on options menu
        optionsMenu.Appear();
    }
    
    /// There is currently no ExitOptionsMenu because the exit-menuing is handled by <see cref="MenuController"/>. Does everything <see cref="EnterOptionsMenu"/> does but in reverse.
}
