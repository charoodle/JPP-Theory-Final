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

    public bool gamePaused { get; protected set; }

    public void BeginGame()
    {
        enemies.SetActive(true);
    }

    public void ResumeGame()
    {
        UnpauseGame();
    }

    public void RestartGame()
    {
        // Timescale = 1 again
        UnpauseGame();

        // Reload current scene; assuming its in build index
        int buildIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(buildIndex);
    }

    public void QuitMainMenu()
    {
        // Timescale = 1 again
        UnpauseGame();

        // Free mouse so player can menu around
        Cursor.lockState = CursorLockMode.None;

        // Idx = 0 = title screen
        SceneManager.LoadScene(0);
    }

    private void Update()
    {
        if(Input.GetKeyDown(PlayerKeybinds.pause))
        {
            if (!gamePaused)
                PauseGame();
            else
                UnpauseGame();
        }
    }

    private void OnEnable()
    {
        // Assume game starts unpaused
        gamePaused = false;

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
        if(winner == playerCastle)
        {
            // Player won - Clear field of all enemies & stop spawning enemies
            enemies.SetActive(false);

            // Display menu
            //  "You win!" - rematch, main menu, exit 

            Debug.Log("GAME OVER - Player won!");
        }
        else if(winner == enemyCastle)
        {
            // Enemy won - Let enemies stay on field
            // Display menu
            // You've fallen... - rematch, main menu, exit

            Debug.Log("GAME OVER - Enemy won...");
        }
    }

    private void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;

        // Bring up player's UI menu
        if(player && player.pauseMenu)
            player.pauseMenu.SetActive(true);
       
        // Show mouse cursor + free movement
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void UnpauseGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;

        // Pause menu disappear
        if(player && player.pauseMenu)
            player.pauseMenu.SetActive(false);

        // Hide + lock cursor for fps game
        Cursor.lockState = CursorLockMode.Locked;
    }
}
