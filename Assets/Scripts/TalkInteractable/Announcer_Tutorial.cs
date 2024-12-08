using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Janky workaround to not make a separate banner-type dialogue manager to announce the beginning of a game.
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
            Talk_BeginGame();
            // Turn off bool in inspector
            beginGame = false;
        }
    }
#endif

    public void Talk_BeginGame()
    {
        StartCoroutine(TalkWithCoroutine());
    }

    protected override IEnumerator TalkWithCoroutine()
    {
        yield return StartTalk();

        yield return TextBox("Voice", $"Welcome! Press {advanceTextKey.ToString()} to advance text.");
        yield return TextBox("I will not repeat myself, so listen up carefully! (Or rather, I can't because I'm limited by whoever programmed me!)");
        yield return TextBox("You can use [W A S D] to move around, and your [Mouse] to look around.");
        yield return TextBox("Use [SPACEBAR] to jump. Use [LEFT SHIFT] to run.");
        yield return TextBox("You know, the standard FPS shooter controls.");
        yield return TextBox("Use the top-row number keys 1-2-3 to switch your weapons between pistol, rocket launcher, and unarmed.");
        yield return TextBox("You will move faster and jump higher when you're unarmed.");
        yield return TextBox("Move ahead to learn more about your weapons at the shooting range.");

        yield return EndTalk();
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
