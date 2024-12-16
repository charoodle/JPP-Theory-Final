using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class Menu : MonoBehaviour
{
    protected GameManager gameManager;
    [SerializeField] TextMeshProUGUI banner;

    

    private void OnEnable()
    {
        if(!gameManager)
            gameManager = FindObjectOfType<GameManager>();
    }



    /// <summary>
    /// By default, makes the menu appear. Does not pause the game by default.
    /// </summary>
    /// <param name="pauseGame">Should the game time be paused?</param>
    public virtual void Appear(bool pauseGame = false)
    {
        MakeMenuAppear(true);
        if(pauseGame)
            PauseGame();
    }

    /// <summary>
    /// By default, makes the menu disappear. Does not unpause game by default.
    /// </summary>
    /// <param name="unpauseGame">Should the game time be unpaused?</param>
    public virtual void Disappear(bool unpauseGame = false)
    {
        MakeMenuAppear(false);
        if(unpauseGame)
            UnpauseGame();
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
