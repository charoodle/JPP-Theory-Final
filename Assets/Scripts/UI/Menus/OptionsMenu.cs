using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OptionsMenu : Menu
{
    public bool playerSavedWhenMenuWasOpen;

    /// <summary>
    /// Appears for a couple seconds when player clicks save button to confirm they saved their settings.
    /// </summary>
    [SerializeField] TMP_Text settingsSavedMessage;

    /// <summary>
    /// Coroutine that makes the tiny "save" confirmation appear and then disappear over time.
    /// </summary>
    protected Coroutine settingsSavedCoroutine;

    /// <summary>
    /// Flag for when player changes any setting in options menu.
    /// </summary>
    protected bool didPlayerChangeAnySetting;

    /// <summary>
    /// Flag for when the message already appeared, warning player they didn't save settings yet.
    /// </summary>
    protected bool didPlayerAlreadySeeNotSavedWarning;


    public override void Appear(bool pauseGame = false)
    {
        base.Appear(pauseGame);

        // Reset this bool each time options menu appears; so can revert if player doesn't save their settings.
        playerSavedWhenMenuWasOpen = false;
    }

    public override Menu Disappear(bool unpauseGame = false)
    {
        // If player did not save values; revert to previous settings
        if (!playerSavedWhenMenuWasOpen)
        {
            ResetSettingsToPrevious();
        }

        // Make menu disappear
        return base.Disappear(unpauseGame);
    }

    /// <summary>
    /// Save the current player settings.
    /// </summary>
    public void SaveSettings()
    {
        Debug.Log("Saving settings to file.");

        string successMessage = "";

        // See if can save player settings to file
        if(PlayerSettings.instance.SaveCurrentValuesToFile())
        { 
            successMessage = "Settings saved!";

            // Set flag to true for the time this menu was open
            playerSavedWhenMenuWasOpen = true;
        }
        // Something went wrong
        else
        {
            successMessage = "Cannot save. Saving failed.";
        }

        // If they save it, then the settings are saved. Reset flag.
        didPlayerChangeAnySetting = false;

        // Make the message appear and fade out over time
        if (settingsSavedCoroutine != null)
            StopCoroutine(settingsSavedCoroutine);
        settingsSavedCoroutine = StartCoroutine(ShowSettingsSavedMessageAndFadeOverTime(successMessage));
    }

    /// <summary>
    /// For UI "X" button.
    /// </summary>
    public void CloseCurrentMenu()
    {
        if(didPlayerChangeAnySetting)
        {
            // Player already saw "settings not saved" warning and still wanted to close menu; so close it
            if (didPlayerAlreadySeeNotSavedWarning)
            {
                menuController.CloseCurrentMenu();
                return;
            }

            // Pop up a small dialogue
            if (settingsSavedCoroutine != null)
                StopCoroutine(settingsSavedCoroutine);
            settingsSavedCoroutine = StartCoroutine(ShowSettingsSavedMessageAndFadeOverTime("(!!!) Settings are unsaved, are you sure?", infiniteShowTime:true));

            // Set flag; player already warned
            didPlayerAlreadySeeNotSavedWarning = true;
        }
        else
            // Player didn't change anything; just close menu.
            menuController.CloseCurrentMenu();
    }



    protected override void OnEnable()
    {
        base.OnEnable();

        // Make message disappear immediately on menu active
        settingsSavedMessage.gameObject.SetActive(false);

        // Reset flags; Option menu just appeared; await for any changes
        didPlayerChangeAnySetting = false;
        // If player changes any settings, make sure they see warning before confirming closing out menu
        didPlayerAlreadySeeNotSavedWarning = false;

        if(PlayerSettings.instance)
            PlayerSettings.instance.playerSettings.OnAnySettingChanged += OnAnyPlayerSettingsChanged;
    }

    protected void OnDisable()
    {
        if(PlayerSettings.instance)
            PlayerSettings.instance.playerSettings.OnAnySettingChanged -= OnAnyPlayerSettingsChanged;
    }

    /// <summary>
    /// Triggered from <see cref="PlayerSettings.PlayerSettingsData.OnAnySettingChanged"/>.
    /// </summary>
    protected void OnAnyPlayerSettingsChanged()
    {
        didPlayerChangeAnySetting = true;
    }



    /// <summary>
    /// Makes a small banner message appear, showing the player that their settings either saved or not.
    /// </summary>
    /// <param name="message">Message to show above the save button.</param>
    /// <param name="messageShowTime">How long to show message for before fading.</param>
    /// <param name="messageFadeTime">How long message takes to fade</param>
    /// <param name="infiniteShowTime">Message will show forever (as long as options menu is up & this coroutine is not stopped).</param>
    /// <returns></returns>
    protected IEnumerator ShowSettingsSavedMessageAndFadeOverTime(string message, float messageShowTime = 0.25f, float messageFadeTime = 1f, bool infiniteShowTime = false)
    {
        // Reset alpha of message
        Color fullAlpha = settingsSavedMessage.color;
        // Note: ALPHA DOES NOT GO FROM [0,255]. IT GOES FROM [0,1].
        fullAlpha.a = 1f;
        settingsSavedMessage.color = fullAlpha;
        settingsSavedMessage.gameObject.SetActive(true);

        // Make sure text is set to correct message
        settingsSavedMessage.text = message;

        /// Optional: Infinite message appearance (until coroutine is manually stopped with another <see cref="ShowSettingsSavedMessageAndFadeOverTime"/> call).
        if(infiniteShowTime)
        {
            while(true)
            {
                yield return null;
            }
        }

        // Game is paused; so have to wait realtime seconds; not scaled seconds.
        // Let it show for a split second before fading
        yield return new WaitForSecondsRealtime(messageShowTime);

        if(messageFadeTime <= 0f)
        {
            // Skip the lerp loop; turn off message
            settingsSavedMessage.gameObject.SetActive(false);
            yield break;
        }

        // Make alpha of message disappear over time
        float timer = 0f;
        Color origColor = settingsSavedMessage.color;
        while(timer < messageFadeTime)
        {
            // Lerp message's alpha from 1 to 0 over time period
            float pct = timer / messageFadeTime;

            Color fadingColor = origColor;

            // Only the alpha should be lerped
            fadingColor.a = Mathf.Lerp(origColor.a, 0f, pct);

            // Set the message's color
            settingsSavedMessage.color = fadingColor;

            // Increase timer, wait for next frame
            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        // Turn off message
        settingsSavedMessage.gameObject.SetActive(false);
    }

    /// <summary>
    /// Load the values from file.
    /// </summary>
    protected void ResetSettingsToPrevious()
    {
        Debug.Log("Player did not save. Resetting settings to previous.");
        PlayerSettings.instance.LoadCurrentValuesFromFile();
    }
}
