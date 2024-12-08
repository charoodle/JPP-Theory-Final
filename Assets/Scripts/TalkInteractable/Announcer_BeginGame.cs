using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Janky workaround to not make a separate banner-type dialogue manager to announce the beginning of a game.
/// </summary>
public class Announcer_BeginGame : TalkWithInteractable
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

        yield return TextBox("Announcer", "The game will commence in...");
        yield return TextBox("3...");
        yield return TextBox("2...");
        yield return TextBox("1...");
        yield return TextBox("Weapons on the ready...");
        yield return TextBox("BEGIN!");

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
