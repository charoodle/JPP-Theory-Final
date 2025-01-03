using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterController = MyProject.CharacterController;

public abstract class TalkWithInteractable : Interactable
{
    protected GameManager gameManager;

    [SerializeField] protected KeyCode advanceTextKey = KeyCode.Mouse0;

    protected const float WAITAFTER_PREVTEXT_DISAPPEAR = 0.25f;
    [SerializeField] protected Transform headLookAt;
    protected DialogueManager dialogue;
    protected CharacterController character;
    protected CharacterController player
    {
        get
        {
            if (!dialogue)
                Debug.LogError("No dialogue object");

            if (!dialogue.Player)
                Debug.LogError("No player object.");

            return dialogue.Player;
        }
    }
    /// <summary>
    /// Is the talk currently running?
    /// </summary>
    public bool isRunning { get; protected set; }

    protected override void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        dialogue = FindObjectOfType<DialogueManager>();
        if(!dialogue)
        {
            Debug.LogError("No dialogue manager found in scene.");
        }
        character = GetComponent<CharacterController>();

        base.Start();
    }

    /// <summary>
    /// Used in other coroutines (ex: cutscenes) where they should wait for the talk to finish.
    /// </summary>
    /// <returns></returns>
    public virtual IEnumerator TalkWith()
    {
        if (!enabled)
            yield break;

        yield return TalkWithCoroutine();
    }

    public override void InteractWith()
    {
        if (!enabled)
            return;

        TalkWith(character);
    }

    protected virtual void TalkWith(CharacterController character)
    {
        //TODO: Remove character? Not used?
        StartCoroutine(TalkWithCoroutine());
    }

    /// <summary>
    /// The main talk coroutine. Can use yield return with:
    /// TextBox
    /// ...
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator TalkWithCoroutine();

    protected bool nextTextBoxCallsAreChildBoxes;

    /// <summary>
    /// Summon a text box on screen and waits for the player before continuing. TextBoxes can also be called from within each other if you use <paramref name="waitCondition"/> as a coroutine and call the textbox from there.
    /// </summary>
    /// <param name="name">Name of the person talking.</param>
    /// <param name="text">What the person is saying.</param>
    /// <param name="minAppearTime">Minimum time the text box will show on screen before letting player continue to next dialogue.</param>
    /// <param name="waitAfterDisappear">How many seconds the box will disappear for before the next dialogue box appears. Helpful in giving a gap between text boxes.</param>
    /// <param name="waitCondition">A custom IEnumerator wait condition. Text box will wait until this condition yield break before continuing to next text box. 
    ///                             <para>If left null, waits for player to click the <see cref="advanceTextKey"/> in the default predicate: <see cref="PlayerWantToProgressToNextTextbox"/></para></param>
    /// <param name="childBox">Does this textbox need to make the previous text box disappear + delay? Useful for textboxes called from within text boxes (conditionals?).</param>
    protected IEnumerator TextBox(string name, string text, float waitAfterDisappear = WAITAFTER_PREVTEXT_DISAPPEAR, float minAppearTime = 0.3f, IEnumerator waitCondition = null)
    {
        // If this is the first text box in call stack, set this call to be the parent text box.
        bool isParentTextBox = false;
        if (!nextTextBoxCallsAreChildBoxes)
        {
            nextTextBoxCallsAreChildBoxes = true;
            // Trigger flag to say only this original text box in call stack is the first text box call.
            isParentTextBox = true;
        }
        // If this textbox is not the parent text box call (ex: recursive textbox calls), then make the previous text disappear and leave a visible time gap before this next textbox.
        else
        {
            dialogue.DisappearTextBox();
            yield return new WaitForSeconds(waitAfterDisappear);
        }

        // Disable the little continue indicator until text box can actually continue
        dialogue.TextBoxEnableContinueIcon(false);

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

        // Force the text box to stay on for a split second before skipping
        yield return new WaitForSeconds(minAppearTime);

        // Enable the little continue indicator again (if no waitCondition provided)
        if(waitCondition == null)
            dialogue.TextBoxEnableContinueIcon(true);

        // Yield wait until player wants to progress to next dialogue box
        yield return WaitForPlayerContinue(waitCondition);

        // Parent text box done displaying.
        if (isParentTextBox)
        {
            // Turn off the text box
            dialogue.DisappearTextBox();

            // Parent leaves gap before next parent text box.
            yield return new WaitForSeconds(waitAfterDisappear);

            // Await for next parent textbox call.
            nextTextBoxCallsAreChildBoxes = false;
        }

        // Done
        yield break;
    }

    /// <summary>Summon a text box, reusing the same name as the immediate previous text box.</summary>
    /// <inheritdoc cref="TextBox"/>
    protected IEnumerator TextBox(string text, float waitAfterDisappear = WAITAFTER_PREVTEXT_DISAPPEAR, float minAppearTime = 0.3f, IEnumerator waitCondition = null)
    {
        // Reuse name from previous text box.
        yield return TextBox(null, text, waitAfterDisappear, minAppearTime, waitCondition);
    }

    protected IEnumerator Pause(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    /// <summary>
    /// Wait for some input during dialogue before progressing to next dialogue box.
    /// </summary>
    protected IEnumerator WaitForPlayerContinue(IEnumerator condition = null)
    {
        // No custom wait condition = user presses button to progress to next text box.
        if (condition == null)
            yield return new WaitUntil(PlayerWantToProgressToNextTextbox);
        // Custom wait condition = code waits for another coroutine to happen before progressing to next text box.
        else
            yield return condition;
    }


    #region TextBox Wait Until Conditions
    /// <summary>
    /// Used for delegates to determine when a text box is allowed to advance.
    /// </summary>
    /// <returns></returns>
    public delegate bool TextBoxWaitUntilCondition();
    #endregion


    protected bool PlayerWantToProgressToNextTextbox()
    {
        // If game is paused, do not progress
        if (gameManager && gameManager.isGamePaused)
            return false;

        // TODO: Figure out when the player wants to progress to next text box. Bool that can check for on and then flip off here?
        return Input.GetKeyDown(advanceTextKey);
    }

    /// <summary>
    /// Turn off movement, look input from controlling character, and weapon firing/switching so player can focus on dialogue box.
    /// </summary>
    /// <param name="enabled"></param>
    protected void EnablePlayerCharacterControl(bool enabled)
    {
        PlayerController player = dialogue.Player as PlayerController;
        player.canInputMove = enabled;
        player.canInputLook = enabled;
        player.canInputJump = enabled;
        player.canFireWeaponInHand = enabled;
        player.canSwitchToPistol = enabled;
        player.canSwitchToRocketLauncher = enabled;
        player.canSwitchToUnarmed = enabled;
    }

    /// <summary>
    /// Call this function to make the character look towards a specific direction/location, and keep tracking target until another LookAt function is called.
    /// </summary>
    /// <param name="character">Character to do the looking.</param>
    /// <param name="target">Transform for character to look towards.</param>
    protected void SimultaneousCharacterLookAt(CharacterController character, Transform target)
    {
        character.LookAt(target);
    }

    /// <inheritdoc cref="SimultaneousCharacterLookAt(CharacterController, Transform)"/>
    /// <param name="yaw">Yaw angle to look at.</param>
    /// <param name="pitch">Pitch angle to look at.</param>
    protected void SimultaneousCharacterLookAt(CharacterController character, float yaw, float pitch)
    {
        StartCoroutine(CharacterLookAt(character, yaw, pitch));
    }

    /// <summary>
    /// Yield return this function during a dialogue section (coroutine) to wait for the character to look at the specific direction/location.
    /// </summary>
    /// <param name="character">Character who will look at something.</param>
    /// <param name="target">What to look at.</param>
    /// <param name="duration">How long to look at the object for.</param>
    /// <returns></returns>
    protected IEnumerator CharacterLookAt(CharacterController character, Transform target, float duration = 1f)
    {
        if (!character)
            throw new System.Exception("Character is null.");

        yield return character.LookAtTargetForSecondsEnum(target, timePeriod:duration, withinDegrees:2f);
    }

    /// <inheritdoc cref="CharacterLookAt(CharacterController, Transform)"/>
    /// <param name="pitch">Pitch direction </param>
    protected IEnumerator CharacterLookAt(CharacterController character, float yaw, float pitch)
    {
        if (!character)
            throw new System.Exception("Character is null.");

        yield return character.LookAtTargetPitchYawEnum(pitch, yaw, withinDegrees: 0.1f);
    }

    protected void GetCharacterLookRotation(CharacterController character, out float yaw, out float pitch)
    {
        character.GetYawAndPitchDegrees(out yaw, out pitch);
    }

    // Save initial character and player rotations.
    float charYaw;
    float charPitch;
    float playerYaw;
    float playerPitch;
    protected virtual IEnumerator StartTalk()
    {
        // Disable player movement and look
        EnablePlayerCharacterControl(false);

        // Disable interaction / interact text for player
        Interactable.showInteractTextOnScreen = false;

        // Save the character's rotations
        character.GetYawAndPitchDegrees(out charYaw, out charPitch);
        player.GetYawAndPitchDegrees(out playerYaw, out playerPitch);

        // Turn on cutscene bars.
        dialogue.ToggleCutsceneBars();

        // Make player and character look at each other during dialogue.
        SimultaneousCharacterLookAt(character, player.head);
        SimultaneousCharacterLookAt(player, character.head);

        isRunning = true;

        yield break;
    }

    protected virtual IEnumerator EndTalk()
    {
        // Turn off cutscene bars.
        dialogue.ToggleCutsceneBars();

        // Make player & character look at their original rotation from before looking at each other.
        // Update: Unused, player looks targets the castle instead of previous look location.
        //      This function would be more helpful like when talking to an NPC in town.
        //SimultaneousCharacterLookAt(character, charYaw, charPitch);
        //yield return CharacterLookAt(player, playerPitch, playerYaw);

        EnablePlayerCharacterControl(true);

        // Enable interaction / interact text for player
        Interactable.showInteractTextOnScreen = true;

        isRunning = false;

        yield break;
    }

    private void OnDisable()
    {
        ExitCutscene();
    }

    public void ExitCutscene()
    {
        // Stop any running talking/looking coroutines.
        StopAllCoroutines();

        // Forcibly exit the cutscene, if its running.
        if (dialogue && isRunning)
            dialogue.CharacterKilled_DisableCutscene();
    }
}
