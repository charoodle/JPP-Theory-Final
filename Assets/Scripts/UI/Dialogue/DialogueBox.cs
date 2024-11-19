using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI textField;

    protected string characterName;
    protected string dialogue;

    public void SetName(string name)
    {
        characterName = name;
    }

    public void SetDialogue(string dialogue)
    {
        this.dialogue = dialogue;
        UpdateBox();
    }

    protected void UpdateBox()
    {
        textField.SetText($"{characterName}: {dialogue}");
    }
}
