using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Janky workaround to not make a separate banner-type dialogue manager to announce the beginning of a game.
/// 
/// Can just call Talk_BeginGame()
/// </summary>
public class Announcer_Tutorial : TalkWithInteractable
{
    [SerializeField] PlayerController playerController;

#if UNITY_EDITOR
    [SerializeField] bool beginGame;
#endif

    private void Awake()
    {
        // Should not be interactable by the player.
        interactText = "NOT PLAYER-INTERACTABLE.";
        playerCanInteractWith = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (beginGame)
        {
            // Don't begin again if running
            if (isRunning)
                return;
            // Start talk
            BeginTalk();
            // Turn off bool in inspector
            beginGame = false;
        }
    }
#endif

    /// <summary>
    /// Since no character, so just circumvent my poorly designed system and call the coroutine with a more verbose function name.
    /// </summary>
    public void BeginTalk()
    {
        StartCoroutine(TalkWithCoroutine());
    }

    protected override IEnumerator TalkWithCoroutine()
    {
        yield return StartTalk();

        yield return TextBox("Voice", $"Welcome! Press {advanceTextKey.ToString()} to advance text.");
        yield return TextBox("I will not repeat myself, so listen up carefully! (Or rather, I can't because I'm limited by whoever programmed me!)", minAppearTime:2.5f);
        yield return TextBox("You can use [W A S D] to move around, and your [Mouse] to look around.", waitCondition: WaitUntilPlayerMovesForward(playerController));
        yield return TextBox("Use [SPACEBAR] to jump. Use [LEFT SHIFT] to run.");
        yield return TextBox("You know, the standard FPS shooter controls.");
        yield return TextBox("Use the top-row number keys 1-2-3 to switch your weapons between pistol, rocket launcher, and unarmed.");
        yield return TextBox("You will move faster and jump higher when you're unarmed.");
        yield return TextBox("Move ahead to learn more about your weapons at the shooting range.");

        yield return EndTalk();
    }

    protected IEnumerator WaitUntilPlayerMovesForward(PlayerController charController)
    {
        float timeWentForward = 0f;
        float timeWentBackward = 0f;
        float timeWentLeft = 0f;
        float timeWentRight = 0f;

        // How long player has to press each direction keys 
        const float enoughSec = 0.65f;

        // Modify these strings to be displayed
        string fwdStr = "Forward: ---\n";
        string backStr = "Backwd: ---\n";
        string leftStr = "Left: ---\n";
        string rightStr = "Right: ---";

        // Concatenate the movement strings into one for info box
        string GetMovementInfoString()
        {
            return fwdStr + backStr + leftStr + rightStr;
        }

        // Check if timers are enough for each movement dir
        //  Timers are updated in while loop
        bool MovedForEnoughSec()
        {
            bool fwdOK = timeWentForward > enoughSec;
            bool backOK = timeWentBackward > enoughSec;
            bool leftOK = timeWentLeft > enoughSec;
            bool rightOK = timeWentRight > enoughSec;

            // Update the string if enough seconds
            if (fwdOK)
                fwdStr = "Forward: OK\n";
            if (backOK)
                backStr = "Backwd: OK\n";
            if (leftOK)
                leftStr = "Left: OK\n";
            if (rightOK)
                rightStr = "Right: OK\n";

            return fwdOK && backOK && leftOK && rightOK;
        }

        // Control an infobox on the screen
        InfoBox infoBox = dialogue.CreateInfoBox();
        infoBox.SetActive(true);

        // Initialize info box position, font sizing, starting text, ...
        infoBox.SetInfo(GetMovementInfoString());
        infoBox.SetInfoFontSize(50f);
        infoBox.SetWindowSize(300f, 315f);
        infoBox.SetWindowAnchorPosition(-600f, 100f);
        infoBox.SetVerticalFontAlignment(InfoBox.VerticalTextAlign.Middle);
        infoBox.SetHorizontalFontAlignment(InfoBox.HorizontalTextAlign.Right);

        // Wait until player presses each direction for enough seconds
        while (!MovedForEnoughSec())
        {
            Vector2 moveInput = charController.moveInput;
            float time = Time.deltaTime;

            // Right
            if (moveInput.x > 0)
                timeWentRight += time;
            // Left
            if (moveInput.x < 0)
                timeWentLeft += time;
            // Forward
            if (moveInput.y > 0)
                timeWentForward += time;
            // Backward
            if (moveInput.y < 0)
                timeWentBackward += time;

            // Did the player move at all?
            if (moveInput.magnitude > 0)
            {
                // Update dialogue window popup here
                infoBox.SetInfo(GetMovementInfoString());
            }

            yield return null;
        } 

        // Final update
        infoBox.SetInfo(GetMovementInfoString() + "Move OK");

        yield return new WaitForSeconds(2f);

        infoBox.SetActive(false);
    }

    /// <summary>
    /// No character to look at; only toggle the cutscene on.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator StartTalk()
    {
        // Turn on cutscene bars.
        dialogue.ToggleCutsceneBars();

        isRunning = true;

        yield break;
    }

    /// <summary>
    /// Only some announcer, not an actual character; toggle cutscene bar off.
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator EndTalk()
    {
        // Turn off cutscene bars.
        dialogue.ToggleCutsceneBars();

        isRunning = false;
        yield break;
    }
}
