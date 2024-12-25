using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    protected GameManager gameManager;
    [SerializeField] protected PauseMenu _pauseMenu;
    [SerializeField] protected GameOverMenu _gameOverMenu;
    public PauseMenu pauseMenu
    {
        get { return _pauseMenu; }
        protected set { _pauseMenu = value; }
    }
    public GameOverMenu gameOverMenu
    {
        get { return _gameOverMenu; }
        protected set { _gameOverMenu = value; }
    }

    public Menu currentMenu;



    public void CloseCurrentMenu()
    {
        // Exit out of currently active Menu; return to parent Menu before it (if no parent, just disable self then and resume game time)
        //  Pause menu will specifically unpause game when it disappears
        Menu parentMenu = currentMenu.Disappear();
        if (parentMenu)
            parentMenu.Appear();
    }

    private void OnEnable()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(PlayerKeybinds.pauseAlt))
        {
            // Game over; don't register keypresses
            if (gameManager.isGameOver)
                return;

            // If not paused, make pause screen appear
            if(!gameManager.isGamePaused)
            {
                pauseMenu.Appear();
            }
            // If already paused (there should be an active currentMenu); then close current menu, return to last menu if there is one
            else
            {
                CloseCurrentMenu();
            }
        }
    }
}
