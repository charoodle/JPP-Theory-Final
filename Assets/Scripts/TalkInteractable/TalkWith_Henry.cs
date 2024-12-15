using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkWith_Henry : TalkWithInteractable
{
    [SerializeField] GameObject enemyCastle;
    [SerializeField] CastleAnimations castleAnims;
    [SerializeField] GameManager gameManager;

    protected override IEnumerator TalkWithCoroutine()
    {
        // Make preparations to present the dialogue between player and this npc
        yield return StartTalk();

        yield return Talk();

        // Make preparations to stop the dialogue between player and this npc
        yield return EndTalk();

        // Remove Henry
        gameObject.SetActive(false);
    }

    protected IEnumerator Talk()
    {
        yield return TextBox("???", "Welcome to the arena!");
        yield return TextBox("Henry", "My name is Henry, I'll be here to facilitate your battle.");
        yield return TextBox("The rules of the arena are simple:");
        yield return TextBox("Take down the enemy castle, and defend your own.");
        yield return TextBox("Whichever castle stays standing wins.");
        yield return TextBox("Are you ready? Your enemy is just about to arrive.");
        
        #region Wait and look at castle as it falls from sky
        // Player+Henry tracks castle while its falling.
        Transform castle = enemyCastle.transform;
        // Make sure castle is active
        castle.gameObject.SetActive(true);
        SimultaneousCharacterLookAt(character, castle);
        yield return Pause(1.5f);
        SimultaneousCharacterLookAt(player, castle);
        yield return castleAnims.CastleFallToGroundAnim();

        // Look back at each other
        yield return TextBox("Don't worry, they're total pushovers. I don't know even who signed off to let them in this tournament.");
        SimultaneousCharacterLookAt(player, character.head);
        yield return Pause(1.0f);
        SimultaneousCharacterLookAt(character, player.head);
        #endregion

        yield return TextBox("I've heard they're not as well equipped, and rely on sending their infinite amount of mindless soldiers as their way of attacking.");
        yield return TextBox("You've got quite the weaponry on your side, however it's quite a shame you're fending your castle by yourself today. Would be much easier with comrades.");
        yield return TextBox("Let's not keep the game waiting any longer.");
        yield return TextBox("Don't forget, take down their castle, and defend your own!");
        yield return TextBox("Good luck! Let the battle commence!");

        // Make player look at enemy caslte
        yield return CharacterLookAt(player, castle);
        // Begin game
        gameManager.BeginGame();
    }

    #region Old
    protected IEnumerator Old()
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
    #endregion
}
