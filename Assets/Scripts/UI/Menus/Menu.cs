using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Menus should be able to be children of another;
///     Ex: Pause > Options > Audio/Mouse/Keybindings
/// </summary>
public class Menu : MonoBehaviour
{
    protected GameManager gameManager;

    /// <summary>
    /// Main contents of the menu.
    /// </summary>
    [SerializeField] protected GameObject contents;

    /// <summary>
    /// Banner text title of the menu.
    /// </summary>
    [SerializeField] TextMeshProUGUI banner;

    protected Image menuBackground;

    /// <summary>
    /// Parent menu if this is child of a menu. Null if this is the root menu.
    /// </summary>
    public Menu parentMenu { get; protected set; }

    protected MenuController menuController;

    

    protected virtual void OnEnable()
    {
        if(!gameManager)
            gameManager = FindObjectOfType<GameManager>();

        // Each menu should have an image as a background
        menuBackground = GetComponent<Image>();

        // Check parent transform for parent menu
        parentMenu = transform.parent.GetComponentInParent<Menu>();

        menuController = GetComponentInParent<MenuController>();

        // All menu object should have a "Contents" child gameobject that contains all the buttons/UI the menu should display, including its title banner.
        if (!contents)
        {
            Transform contentsChild = transform.Find("Contents");
            contents = contentsChild.gameObject;

            string err = "No contents object assigned on this menu.";
            if(!contents)
                err += " No Contents object found as a child either.";
            Debug.LogError(err, this.gameObject);
        }
    }



    /// <summary>
    /// By default, makes the menu appear. Does not pause the game by default.
    /// </summary>
    /// <param name="pauseGame">Should the game time be paused?</param>
    public virtual void Appear(bool pauseGame = false)
    {
        // Init if not yet init
        if (!menuController)
        {
            OnEnable();
        }

        // Assign this window as current menu
        menuController.currentMenu = this;
        // Make this window appear
        MakeMenuAppear(true);
        // Pause game, optionally
        if(pauseGame)
            PauseGame();
    }

    /// <summary>
    /// By default, makes the menu disappear. Does not unpause game by default. This should always be called before <see cref="Appear"/>.
    /// </summary>
    /// <param name="unpauseGame">Should the game time be unpaused?</param>
    /// <returns>Parent menu. Null if no parent menu.</returns>
    public virtual Menu Disappear(bool unpauseGame = false)
    {
        menuController.currentMenu = null;

        MakeMenuAppear(false);
        if(unpauseGame)
            UnpauseGame();

        return parentMenu;
    }

    /// <summary>
    /// Change what the banner text of the menu says.
    /// </summary>
    /// <param name="text"></param>
    public virtual void SetBannerText(string text)
    {
        banner.text = text;
    }



    /// <summary>
    /// Show the menu on the player's screen.
    /// </summary>
    /// <param name="appear"></param>
    protected virtual void MakeMenuAppear(bool appear)
    {
        // Make the menu contents + background appear again
        contents.SetActive(appear);
        menuBackground.enabled = appear;

        // Make menu appear/disappear
        gameObject.SetActive(appear);
    }

    /// <summary>
    /// Pause the game's time.
    /// </summary>
    protected void PauseGame()
    {
        gameManager.PauseGameTime();
    }

    /// <summary>
    /// Resume game time.
    /// </summary>
    protected void UnpauseGame()
    {
        gameManager.UnpauseGameTime();
    }
}
