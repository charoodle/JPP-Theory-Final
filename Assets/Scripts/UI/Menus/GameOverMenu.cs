using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverMenu : Menu
{
    /// <summary>
    /// By default, a menu appearing will also pause the game.
    /// </summary>
    /// <param name="pauseGame">Should the game time be paused?</param>
    /// <param name="setBannerText">If set, will set banner text of game over menu to this.</param>
    public virtual void Appear(bool pauseGame = false, string setBannerText = "")
    {
        if (setBannerText != "")
            SetBannerText(setBannerText);

        base.Appear(pauseGame);
    }
}
