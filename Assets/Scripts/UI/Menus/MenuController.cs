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



    private void OnEnable()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    protected void Update()
    {
        if (Input.GetKeyDown(PlayerKeybinds.pause) && !gameManager.isGameOver)
        {
            if (!gameManager.isGamePaused)
                pauseMenu.Appear(pauseGame: true);
            else
                pauseMenu.Disappear(unpauseGame: true);
        }
    }
}
