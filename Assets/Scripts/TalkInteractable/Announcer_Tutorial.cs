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
        const float enoughSec = 1.5f;

        bool MovedForEnoughSec()
        {
            return timeWentForward  > enoughSec &&
                   timeWentBackward > enoughSec &&
                   timeWentLeft     > enoughSec &&
                   timeWentRight    > enoughSec;
        }

        // Wait until player presses each direction enough
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
