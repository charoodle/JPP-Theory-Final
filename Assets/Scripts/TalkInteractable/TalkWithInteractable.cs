using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    protected IEnumerator ExampleFlow_TalkWithCoroutine()
    {
        yield return TextBox("The character says something here.");

        yield return TextBox("The character says another thing here.");

        yield break;
    }

    protected IEnumerator WaitForPlayerContinue()
    {
        yield return new WaitUntil(PlayerWantToProgressToNextTextbox);
    }

    public bool PlayerWantToProgressToNextTextbox()
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
        // Turn off movement, look input from controlling character

        // Turn off crosshair
    }

    protected void CharacterLookAt(MyProject.CharacterController character, Transform target)
    {
    }
}
