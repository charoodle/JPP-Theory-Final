using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CharacterController = MyProject.CharacterController;

public class DialogueManager : MonoBehaviour
{
    public DialogueBox box;
    public InfoBox infoBox;

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

    #region InfoBox
    /// <summary>
    /// Give a caller control over an info box on the player's screen.
    /// TODO: Be able to instantiate multiple if needed?
    /// </summary>
    public InfoBox CreateInfoBox()
    {
        return infoBox;
    }
    #endregion

    /// <summary>
    /// Creates a dialogue-like text box across the bottom of the screen. Includes a name and some
    /// dialogue for that character that is speaking.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="startDialogue"></param>
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

    /// <summary>
    /// Turn on/off the little icon that indicates the player can progress to the next text box.
    /// </summary>
    /// <param name="enabled"></param>
    public void TextBoxEnableContinueIcon(bool enabled)
    {
        box.EnableContinueIcon(enabled);
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
    
    /// <summary>
    /// When the character the player is currently talking to is killed or disabled suddenly, give player control and pull them out of the cutscene.
    /// </summary>
    public void CharacterKilled_DisableCutscene()
    {
        if(cutsceneBarsAreEnabled)
            ToggleCutsceneBars();

        PlayerController player = _player as PlayerController;
        if (player)
        {
            player.canInputLook = true;
            player.canInputMove = true;
        }
        
        Interactable.showInteractTextOnScreen = true;

        // Disable any text boxes
        DisappearTextBox();
    }
}
