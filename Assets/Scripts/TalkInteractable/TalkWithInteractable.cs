using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterController = MyProject.CharacterController;

public abstract class TalkWithInteractable : Interactable
{
    [SerializeField] protected Transform headLookAt;

    [SerializeField] protected string _npcName;
    protected string npcName
    {
        get { return _npcName; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.LogError("NPC Name cannot be empty.");
                return;
            }
            _npcName = value;
        }
    }

    protected DialogueManager dialogue;

    protected override void Start()
    {
        dialogue = FindObjectOfType<DialogueManager>();
        if(!dialogue)
        {
            Debug.LogError("No dialogue manager found in scene.");
        }

        base.Start();
    }

    public override void InteractWith()
    {
        TalkWith();
    }

    protected virtual void TalkWith()
    {
        Debug.Log("Talk with: " + npcName);
        StartCoroutine(TalkWithCoroutine());
    }

    protected abstract IEnumerator TalkWithCoroutine();

    /// <summary>
    /// Summon a text box on screen and waits for the player before continuing.
    /// </summary>
    /// <param name="name">Name of the person talking.</param>
    /// <param name="text">What the person is saying.</param>
    protected IEnumerator TextBox(string name, string text)
    {
        // No name passed in = set just the text box.
        if(string.IsNullOrEmpty(name))
        {
            dialogue.NextDialogue(text);
        }
        else
        {
            // Set text box with the name passed in.
            dialogue.CreateTextBox(name, text);
        }

        // Yield wait until player wants to progress to next dialogue box
        yield return WaitForPlayerContinue();

        // Turn off the text box
        dialogue.DisappearTextBox();

        // Gap between text boxes
        yield return new WaitForSeconds(0.25f);

        // Done
        yield break;
    }

    /// <summary>Summon a text box, reusing the same name as the immediate previous text box.</summary>
    /// <inheritdoc cref="TextBox(string, string)"/>
    protected IEnumerator TextBox(string text)
    {
        // Reuse name from previous text box.
        yield return TextBox(null, text);
    }

    /// <summary>
    /// Wait for some input during dialogue before progressing to next dialogue box.
    /// </summary>
    protected IEnumerator WaitForPlayerContinue()
    {
        yield return new WaitUntil(PlayerWantToProgressToNextTextbox);
    }

    protected bool PlayerWantToProgressToNextTextbox()
    {
        // TODO: Figure out when the player wants to progress to next text box. Bool that can check for on and then flip off here?
        return Input.GetMouseButtonDown(0);
    }

    protected IEnumerator EnableCutsceneBlackBars(bool enabled)
    {
        // 

        yield break;
    }

    protected void EnablePlayerCharacterControl(bool enabled)
    {
        PlayerController player = dialogue.Player as PlayerController;

        // Turn off movement, look input from controlling character
        player.canInputMove = enabled;
        player.canInputLook = enabled;
    }

    bool endTalk = false;
    protected IEnumerator CharacterLookAtUntilEndOfTalk(MyProject.CharacterController character, Transform target)
    {
        StartCoroutine(character.LookAtTargetAndThenBackUntil(() => endTalk, target, 0.3f));
        yield break;
    }

    protected IEnumerator StartTalk()
    {
        // Make player look at target until end of talk
        StartCoroutine(CharacterLookAtUntilEndOfTalk(dialogue.Player, headLookAt));
        EnablePlayerCharacterControl(false);
        endTalk = false;

        // Make character controller look at initiater until end of talk
        Transform playerHead = dialogue.Player.head;
        CharacterController npc = GetComponent<CharacterController>();
        StartCoroutine(CharacterLookAtUntilEndOfTalk(npc, playerHead));

        // Disable interaction / interact text for player
        Interactable.showInteractTextOnScreen = false;

        yield break;
    }

    protected IEnumerator EndTalk()
    {
        endTalk = true;
        EnablePlayerCharacterControl(true);

        // TODO: Wait until player returns to previous look, and then enable player control.

        // Enable interaction / interact text for player
        Interactable.showInteractTextOnScreen = true;

        yield break;
    }
}
