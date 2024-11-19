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
        // TODO: Create and show a text box on screen with name, contents, ...
        // ...

        // Yield wait until player wants to progress to next dialogue box
        yield return new WaitUntil(PlayerWantToProgressToNextTextbox);

        // TODO: Turn off the text box
        // ...

        // Done
        yield break;
    }

    protected IEnumerator ExampleFlow_TalkWithCoroutine()
    {
        yield return TextBox("The character says something here.");

        yield return TextBox("The character says another thing here.");

        yield break;
    }

    

    public bool PlayerWantToProgressToNextTextbox()
    {
        // TODO: Figure out when the player wants to progress to next text box. Bool that can check for on and then flip off here?
        return false;
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
