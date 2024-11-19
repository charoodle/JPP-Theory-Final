using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Something interactable by the player
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    protected PlayerUI playerUI;
    public string interactText;
    protected Camera playerCam;
    protected Coroutine mouseEnterCoroutine;

    /// <summary>
    /// Force the text to be on and off, especially during dialogue.
    /// </summary>
    public static bool showInteractTextOnScreen = true;

    protected virtual void Start()
    {
        playerCam = Camera.main;
        playerUI = FindObjectOfType<PlayerUI>();
    }

    public abstract void InteractWith();

    protected void OnMouseEnter()
    {
        if (mouseEnterCoroutine != null)
            StopCoroutine(mouseEnterCoroutine);
        mouseEnterCoroutine = StartCoroutine(ShowInteractTextIfInRange());
    }

    protected void OnMouseExit()
    {
        DisableInteractText();
    }

    private void OnDisable()
    {
        DisableInteractText();
    }

    private void DisableInteractText()
    {
        if (mouseEnterCoroutine != null)
            StopCoroutine(mouseEnterCoroutine);

        if(playerUI)
        {
            playerUI.EnableInteractText(false);
            playerUI.SetInteractText("");
        }
    }

    private IEnumerator ShowInteractTextIfInRange()
    {
        while(true)
        {
            // Do not show on screen if disabled
            if(!showInteractTextOnScreen)
            {
                playerUI.EnableInteractText(false);
                yield return null;
                continue;
            }

            // Constantly check if player is in range
            if(IsPlayerInRangeToInteract())
            {
                playerUI.EnableInteractText(true);
                playerUI.SetInteractText("[E] " + interactText);
            }
            else
            {
                playerUI.EnableInteractText(false);
                playerUI.SetInteractText("");
            }
            yield return null;
        }
    }

    protected bool IsPlayerInRangeToInteract()
    {
        float dist = Vector3.Distance(playerCam.transform.position, transform.position);
        return dist <= PlayerController.interactRange;       
    }
}
