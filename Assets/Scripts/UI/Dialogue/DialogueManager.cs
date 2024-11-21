using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CharacterController = MyProject.CharacterController;

public class DialogueManager : MonoBehaviour
{
    public DialogueBox box;

    protected CharacterController _player;
    public CharacterController Player
    {
        get
        {
            if(!_player)
                throw new System.Exception("Dialogue: No player controller found in scene.");
            return _player;
        }
        private set { _player = value; }
    }

    #region Cutscene Bars
    /// <summary>
    /// Bottom cutscene bar.
    /// </summary>
    [SerializeField] RectTransform cutsceneBarB;
    /// <summary>
    /// Top cutscene bar.
    /// </summary>
    [SerializeField] RectTransform cutsceneBarT;
    /// <summary>
    /// Used for knowing whether state of cutscene bars is enabled/disabled.
    /// </summary>
    bool enableCutsceneBars = true;
    /// <summary>
    /// Are the cutscene bars currently enabled/disabled?
    /// </summary>
    public bool cutsceneBarsAreEnabled
    {
        get { return !enableCutsceneBars; }
    }
    #endregion

    private void Start()
    {
        Player = FindObjectOfType<PlayerController>();
    }

    public void CreateTextBox(string name, string startDialogue)
    {
        // Set the box
        box.SetName(name);
        box.SetDialogue(startDialogue);

        // Turn on dialogue box
        box.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Make the text box with dialogue, but keep same name as previously set.
    /// </summary>
    /// <param name="dialogue"></param>
    public void NextDialogue(string dialogue)
    {
        // If for some reason text box is not on, turn it on
        if (!box.gameObject.activeInHierarchy)
            box.gameObject.SetActive(true);

        box.SetDialogue(dialogue);
    }

    /// <summary>
    /// Turn off the dialogue box.
    /// </summary>
    public void DisappearTextBox()
    {
        box.gameObject.SetActive(false);
    }

    public void ToggleCutsceneBars()
    {
        StopAllCoroutines();
        StartCoroutine(ToggleCutsceneBarsCoroutine());
    }

    /// <summary>
    /// Assumes cutscene bars are an Image that has 60 height.
    /// </summary>
    /// <returns></returns>
    public IEnumerator ToggleCutsceneBarsCoroutine()
    {
        const float timeToComplete = 0.5f;
        float timer = 0f;

        Vector2 enabledPosB = Vector2.zero;
        Vector2 disabledPosB = new Vector2(0f, -60f);

        Vector2 enabledPosT = Vector2.zero;
        Vector2 disabledPosT = new Vector2(0f, 60f);

        while (timer < timeToComplete)
        {
            float pct = timer / timeToComplete;

            // Enable cutscene bars
            if(enableCutsceneBars)
            {
                cutsceneBarB.anchoredPosition = Vector2.Lerp(disabledPosB, enabledPosB, pct);
                cutsceneBarT.anchoredPosition = Vector2.Lerp(disabledPosT, enabledPosT, pct);
            }
            // Disable cutscene bars
            else
            {
                cutsceneBarB.anchoredPosition = Vector2.Lerp(enabledPosB, disabledPosB, pct);
                cutsceneBarT.anchoredPosition = Vector2.Lerp(enabledPosT, disabledPosT, pct);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (enableCutsceneBars)
        {
            cutsceneBarB.anchoredPosition = enabledPosB;
            cutsceneBarT.anchoredPosition = enabledPosT;
        }
        else
        {
            cutsceneBarB.anchoredPosition = disabledPosB;
            cutsceneBarT.anchoredPosition = disabledPosT;
        }

        // Flip for next toggle.
        enableCutsceneBars = !enableCutsceneBars;

        yield break;
    }
}
