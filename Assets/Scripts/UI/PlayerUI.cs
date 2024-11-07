using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI interactText;

    private void Start()
    {
        EnableInteractText(false);
    }

    public void SetInteractText(string text)
    {
        interactText.SetText(text);
    }

    public void EnableInteractText(bool value)
    {
        interactText.gameObject.SetActive(value);
    }
}
