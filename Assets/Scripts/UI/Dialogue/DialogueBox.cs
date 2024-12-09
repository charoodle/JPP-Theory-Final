using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// TODO: Inherit from InfoBox maybe? Have to rework in that case. Only if time allows.
/// </summary>
public class DialogueBox : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI textField;
    [SerializeField] protected GameObject continueIcon;

    protected string characterName;
    protected string dialogue;

    public void SetName(string name)
    {
        characterName = name;
    }

    public void SetDialogue(string dialogue)
    {
        if (string.IsNullOrWhiteSpace(characterName))
        {
            Debug.LogError("Dialogue: Character name not set.", this.gameObject);
            return;
        }

        this.dialogue = dialogue;
        UpdateBox();
    }

    /// <summary>
    /// Turns on/off the icon on the bottom right of the box that indicates the player can progress to the next text box with the appropriate button press.
    /// </summary>
    /// <param name="enable"></param>
    public void EnableContinueIcon(bool enable)
    {
        continueIcon.SetActive(enable);
    }

    protected void UpdateBox()
    {
        textField.SetText($"{characterName}: {dialogue}");
    }
}
