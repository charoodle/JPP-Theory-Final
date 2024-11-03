using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour
{
    public const int DemoSceneBuildIdx = 1;

    public void StartDemoScene()
    {
        // build index 1 = demo scene
        SceneManager.LoadScene(DemoSceneBuildIdx);
    }
}
