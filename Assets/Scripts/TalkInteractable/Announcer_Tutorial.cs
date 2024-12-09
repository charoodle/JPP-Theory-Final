using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Avoid using Unity.CharacterController
using CharacterController = MyProject.CharacterController;

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
        if (Application.isPlaying && beginGame)
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
        yield return TextBox("This is your first time here, right? Let me teach you the basics.");
        yield return TextBox("I will not repeat myself, so listen up very carefully!", minAppearTime:2.5f);
        yield return TextBox("You can use [W A S D] to move around, and your [Mouse] to look around.", waitCondition: WaitUntilCharacterMovesAndLooksAround(playerController));
        yield return TextBox("Press [SPACEBAR] to jump.", waitCondition: WaitUntilCharacterJumps(playerController));
        yield return TextBox("Hold [LEFT SHIFT] to run.", waitCondition: WaitUntilCharacterRuns(playerController));
        yield return TextBox("Fairly standard FPS shooter controls.");
        yield return TextBox("Use the top-row number keys 1-2-3 to switch your weapons between pistol, rocket launcher, and unarmed.");
        yield return TextBox("You will move faster and jump higher when you're unarmed.");
        yield return TextBox("Move ahead to learn more about your weapons at the shooting range.");

        yield return EndTalk();
    }

    /// <summary>
    /// Detects whether the character has...
    ///     Moved around in all 4 directions for enough seconds.
    ///     Looked around (any input) for enough seconds.
    /// Also pulls up an info box on player's screen to show player if they've done the action or not for each of those two steps.
    /// </summary>
    /// <param name="charController"></param>
    protected IEnumerator WaitUntilCharacterMovesAndLooksAround(CharacterController charController)
    {
        #region Wait until player moves around
        float timeWentForward = 0f;
        float timeWentBackward = 0f;
        float timeWentLeft = 0f;
        float timeWentRight = 0f;

        // How long player has to press each direction keys 
        float enoughSec = 0.65f;

        // Modify these strings to be displayed
        string fwdStr = "<color=white>Forward: --\n";
        string backStr = "<color=white>Backwd: --\n";
        string leftStr = "<color=white>Left: --\n";
        string rightStr = "<color=white>Right: --";

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
                fwdStr = "<color=white>Forward: <color=green>OK\n";
            if (backOK)
                backStr = "<color=white>Backwd: <color=green>OK\n";
            if (leftOK)
                leftStr = "<color=white>Left: <color=green>OK\n";
            if (rightOK)
                rightStr = "<color=white>Right: <color=green>OK\n";

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
        infoBox.SetInfo(GetMovementInfoString());
        #endregion

        // Gap between boxes
        const float gapBetweenBoxes = 2f;
        yield return new WaitForSeconds(gapBetweenBoxes);

        // Turn box off. (Maybe destroy it?)
        infoBox.SetActive(false);

        yield return new WaitForSeconds(gapBetweenBoxes);


        #region Look Around
        string lookAroundStr = "Look Around: --\n";

        // Turn box on
        infoBox.SetActive(true);
        // Initialize info box position, font sizing, starting text, ...
        infoBox.SetInfo(lookAroundStr);
        infoBox.SetInfoFontSize(50f);
        infoBox.SetWindowSize(400f, 90f);
        infoBox.SetWindowAnchorPosition(-600f, 100f);
        infoBox.SetVerticalFontAlignment(InfoBox.VerticalTextAlign.Middle);
        infoBox.SetHorizontalFontAlignment(InfoBox.HorizontalTextAlign.Right);

        float timeLookAround = 0f;
        enoughSec = 2f;
        while(timeLookAround < enoughSec)
        {
            Vector2 lookInput = charController.lookInput;
            if (lookInput.magnitude > 0)
                timeLookAround += Time.deltaTime;
            yield return null;
        }

        // Player looked around enough, update info box and let player see for a couple sec
        infoBox.SetInfo("<color=white>Look Around: <color=green>OK\n");

        yield return new WaitForSeconds(gapBetweenBoxes);

        // Turn box off.
        infoBox.SetActive(false);

        yield return new WaitForSeconds(gapBetweenBoxes);

        #endregion
    }

    /// <summary>
    /// Detects when the character has jumped once.
    /// </summary>
    protected IEnumerator WaitUntilCharacterJumps(CharacterController charController)
    {
        bool playerJumped = false;
        void OnPlayerJump()
        {
            playerJumped = true;
        }

        charController.OnCharacterJump += OnPlayerJump;

        while(!playerJumped)
        {
            // Wait for player to jump
            yield return new WaitForFixedUpdate();
        }

        charController.OnCharacterJump -= OnPlayerJump;

        yield return new WaitForSeconds(2f);

        yield break;
    }

    /// <summary>
    /// Detects when the character has run for some seconds.
    /// </summary>
    protected IEnumerator WaitUntilCharacterRuns(CharacterController charController)
    {
        const float enoughSec = 2f;
        float runTimer = 0f;
        while(runTimer < enoughSec)
        {
            if (charController.isSprinting)
                runTimer += Time.deltaTime;

            yield return null;
        }
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
