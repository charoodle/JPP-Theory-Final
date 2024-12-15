using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject enemies;
    [SerializeField] Castle playerCastle;
    [SerializeField] Castle enemyCastle;

    public void BeginGame()
    {
        enemies.SetActive(true);
    }

    private void OnEnable()
    {
        if (playerCastle)   playerCastle.OnCastleDestroyed += OnCastleDestroyed;
        if (enemyCastle)    enemyCastle.OnCastleDestroyed  += OnCastleDestroyed;
    }

    private void OnDisable()
    {
        if (playerCastle) playerCastle.OnCastleDestroyed -= OnCastleDestroyed;
        if (enemyCastle)  enemyCastle.OnCastleDestroyed  -= OnCastleDestroyed;
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
}
