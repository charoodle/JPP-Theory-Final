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
        yield return TextBox("You can use [W A S D] to move around, and your [Mouse] to look around.", waitCondition: PlayerMovesAround);

        // Wait for the player to move around

        yield return TextBox("Use [SPACEBAR] to jump. Use [LEFT SHIFT] to run.");
        yield return TextBox("You know, the standard FPS shooter controls.");
        yield return TextBox("Use the top-row number keys 1-2-3 to switch your weapons between pistol, rocket launcher, and unarmed.");
        yield return TextBox("You will move faster and jump higher when you're unarmed.");
        yield return TextBox("Move ahead to learn more about your weapons at the shooting range.");

        yield return EndTalk();
    }

    Coroutine waitUntilPlayerMovesCoroutine;
    bool waitingForCoroutine = false;
    /// <summary>
    /// Uses coroutines and a bool to determine if the player has satisfied the "move in all directions" condition of tutorial.
    /// // What the heck is this monstrosity.
    /// // This delegate check works for simple bools, but for checks over time (ex: move in all four directions for at least 1 second each since text box appeared) it's not really that good nor easy to understand.
    /// 
    /// Somehow need a way to tell the text box function:
    ///     Pause text box progress until this coroutine finishes.
    /// 
    /// </summary>
    /// <returns></returns>
    protected bool PlayerMovesAround()
    {
        if(!waitingForCoroutine)
        {
            // Start coroutine if its not started yet.
            Debug.Log("Starting move coroutine");
            waitUntilPlayerMovesCoroutine = StartCoroutine(WaitUntilPlayerMovesForward());
            waitingForCoroutine = true;

            // Just started, so player probably didn't move around enough.
            return false;
        }
        else
        {
            // If coroutine is null, coroutine must've finished from player fulfilling condition.
            if (waitUntilPlayerMovesCoroutine == null)
            {
                Debug.Log("Player fulfilled condition!");
                waitingForCoroutine = false;
                return true;
            }
            else
            {
                Debug.LogWarning("Waiting for player condition...");
                // Waiting for coroutine to finish
                return false;
            }
        }
    }

    protected IEnumerator WaitUntilPlayerMovesForward()
    {
        float timePressedForward = 0f;

        // Wait until player presses forward for enough seconds
        while (timePressedForward < 1f)
        {
            if(Input.GetKey(KeyCode.W))
            {
                timePressedForward += Time.deltaTime;
            }

            yield return null;
        }

        /// Null it out to indicate to <see cref="PlayerMovesAround"/> that the move condition has been satisfied.
        waitUntilPlayerMovesCoroutine = null;
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
