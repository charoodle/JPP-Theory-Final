using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterController = MyProject.CharacterController;

public abstract class TalkWithInteractable : Interactable
{
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

    protected IEnumerator TextBox(string text)
    {
        // Reuse name from previous text box.
        yield return TextBox(null, text);
    }

    protected IEnumerator TextBox(string name, string text)
    {
        // No name passed in = set just the text box.
        if(string.IsNullOrEmpty(name))
        {
            dialogue.NextDialogue(text);
        }
        else
        {
            // Set text box with a name.
            dialogue.CreateTextBox(npcName, text);
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

    /// <summary>
    /// Wait for some input during dialogue before progressing to next dialogue box.
    /// </summary>
    /// <returns></returns>
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
        StartCoroutine(character.LookAtTargetAndThenBackUntil(() => endTalk, this.transform));
        yield break;
    }

    protected IEnumerator StartTalk(Transform target)
    {
        // Make player look at target until end of talk
        StartCoroutine(CharacterLookAtUntilEndOfTalk(dialogue.Player, target));
        EnablePlayerCharacterControl(false);
        endTalk = false;

        // Disable interaction / interact text

        yield break;
    }

    protected IEnumerator EndTalk()
    {
        endTalk = true;
        EnablePlayerCharacterControl(true);
        yield break;

        // Enable interaction / interact text
    }
}
