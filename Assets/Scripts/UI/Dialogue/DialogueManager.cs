using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CharacterController = MyProject.CharacterController;

public class DialogueManager : MonoBehaviour
{
    public DialogueBox box;

    protected CharacterController _player;
    public CharacterController Player
    {
        get
        {
            if(!_player)
                throw new System.Exception("Dialogue: No player controller found in scene.");
            return _player;
        }
        private set { _player = value; }
    }

    private void Start()
    {
        Player = FindObjectOfType<PlayerController>();
    }

    public void CreateTextBox(string name, string startDialogue)
    {
        // Set the box
        box.SetName(name);
        box.SetDialogue(startDialogue);

        // Turn on dialogue box
        box.gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Make the text box with dialogue, but keep same name as previously set.
    /// </summary>
    /// <param name="dialogue"></param>
    public void NextDialogue(string dialogue)
    {
        // If for some reason text box is not on, turn it on
        if (!box.gameObject.activeInHierarchy)
            box.gameObject.SetActive(true);

        box.SetDialogue(dialogue);
    }

    /// <summary>
    /// Turn off the dialogue box.
    /// </summary>
    public void DisappearTextBox()
    {
        box.gameObject.SetActive(false);
    }
}
