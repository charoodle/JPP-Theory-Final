using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkWith_Henry : TalkWithInteractable
{
    [SerializeField] GameObject enemyCastle;
    [SerializeField] CastleAnimations castleAnims;

    protected override IEnumerator TalkWithCoroutine()
    {
        // Make preparations to present the dialogue between player and this npc
        yield return StartTalk();

        yield return HenryIntro();

        yield return Pause(0.25f);

        yield return CastleIntro();

        // Make preparations to stop the dialogue between player and this npc
        yield return EndTalk();
    }

    protected IEnumerator HenryIntro()
    {
        // Run through dialogue action
        yield return TextBox("???", "Hello dear traveler!");

        yield return TextBox("Henry", "My name is Henry.");

        yield return TextBox("Welcome to my kingdom.");

        yield return TextBox("It's not much, but please make yourself at home on this cozy little island.");
    }

    protected IEnumerator CastleIntro()
    {
        yield return TextBox("Oh, actually I do have a quest for you.");

        yield return TextBox("Do you see that castle falling from the sky just over there?");

        // Player+Henry tracks castle while its falling.
        Transform castle = enemyCastle.transform;
        // Make sure castle is active
        castle.gameObject.SetActive(true);
        SimultaneousCharacterLookAt(character, castle);
        yield return Pause(1.5f);
        SimultaneousCharacterLookAt(player, castle);
        yield return castleAnims.CastleFallToGroundAnim();

        yield return TextBox("Yes, that castle.");
        yield return TextBox("Do you think you could destroy them for us?");

        // Look back at each other
        SimultaneousCharacterLookAt(player, character.head);
        yield return Pause(1.0f);
        SimultaneousCharacterLookAt(character, player.head);

        yield return TextBox("Of course, not without the proper payment and backing from my kingdom.");
        yield return TextBox("We shall supply you and your crew with the finest...");
        yield return TextBox("*ahem*");
        yield return TextBox("...state of the art weaponry.");
        yield return TextBox("Do be careful, however. It's still experimental.");
        yield return TextBox("Good luck!");
    }
}
