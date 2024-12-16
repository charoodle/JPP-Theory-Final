using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public const int TutorialBuildIdx = 1;
    public const int GameSceneBuildIdx = 2;

    [SerializeField] GameObject exitGameButton;

    private void Start()
    {
#if UNITY_WEBGL
        // No exiting in webgl
        exitGameButton.SetActive(false);
#endif
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene(TutorialBuildIdx);
    }

    public void StartGameScene()
    {
        SceneManager.LoadScene(GameSceneBuildIdx);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
