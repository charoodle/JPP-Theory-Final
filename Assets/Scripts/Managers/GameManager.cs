using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Player player;
    [SerializeField] GameObject enemies;
    [SerializeField] Castle playerCastle;
    [SerializeField] Castle enemyCastle;

    public bool isGamePaused { get; protected set; }
    public bool isGameOver { get; protected set; }



    public void BeginGame()
    {
        // Enemies will start spawning when script turns on
        enemies.SetActive(true);
    }

    public void ResumeGame()
    {
        UnpauseGameTime();
        player.menus.pauseMenu.Disappear(unpauseGame: true);
    }

    public void RestartGame()
    {
        // Timescale = 1 again
        UnpauseGameTime();

        // Reload current scene; assuming its in build index
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(buildIndex);
    }

    public void QuitMainMenu()
    {
        // Timescale = 1 again
        UnpauseGameTime();

        // Free mouse so player can menu around
        Cursor.lockState = CursorLockMode.None;

        // Idx = 0 = title screen
        SceneManager.LoadScene(0);
    }



    private void OnEnable()
    {
        // Assume game starts unpaused
        isGamePaused = false;

        player = FindObjectOfType<Player>();

        if (playerCastle)   
            playerCastle.OnCastleDestroyed += OnCastleDestroyed;
        if (enemyCastle)    
            enemyCastle.OnCastleDestroyed  += OnCastleDestroyed;
    }

    private void OnDisable()
    {
        if (playerCastle) 
            playerCastle.OnCastleDestroyed -= OnCastleDestroyed;
        if (enemyCastle)  
            enemyCastle.OnCastleDestroyed  -= OnCastleDestroyed;
    }

    private void OnCastleDestroyed(Castle castle)
    {
        if (castle == enemyCastle)
        {
            EndGame(playerCastle);
        }
        else if(castle == playerCastle)
        {
            EndGame(enemyCastle);
        }
    }

    private void EndGame(Castle winner)
    {
        isGameOver = true;

        if(winner == playerCastle)
        {
            // Player won - Clear field of all enemies & stop spawning enemies
            enemies.SetActive(false);

            // Display menu
            //  "You win!" - rematch, main menu, exit 
            player.menus.gameOverMenu.Appear(setBannerText: "Game over - you win!", pauseGame: true);

            Debug.Log("GAME OVER - Player won!");
        }
        else if(winner == enemyCastle)
        {
            // Player lost - Clear field of all enemies & stop spawning enemies
            enemies.SetActive(false);

            // Enemy won - Let enemies stay on field
            // Display menu
            // You've fallen... - rematch, main menu, exit
            player.menus.gameOverMenu.Appear(setBannerText: "You've fallen...", pauseGame: true);

            Debug.Log("GAME OVER - Enemy won...");
        }
    }

    /// <summary>
    /// Freeze game time + unlock cursor.
    /// </summary>
    public void PauseGameTime()
    {
        isGamePaused = true;
        Time.timeScale = 0f;

        CursorShowAndFree();
    }

    /// <summary>
    /// Unfreeze game time to normal + lock cursor.
    /// </summary>
    public void UnpauseGameTime()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        CursorLock();
    }

    public void CursorLock()
    {
        // Hide + lock cursor for fps game
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void CursorShowAndFree()
    {
        // Show mouse cursor + free movement
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
