using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueBox box;

    private void Start()
    {
        box.SetName("Henry");
        box.SetDialogue("Hello, my name is Henry!");
    }
}
