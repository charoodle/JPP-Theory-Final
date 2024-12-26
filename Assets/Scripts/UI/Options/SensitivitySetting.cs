using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// A single sensitivity setting in the options menu.
/// Wow. That's a lot of code.
/// TODO: See if can refactor to make this more general for each setting as necessary?
/// </summary>
public class SensitivitySetting : MonoBehaviour
{
    // Default value
    public const float LOOKSENS_DEFAULT = 3f;
    public const float LOOKSENS_MIN = 0.01f;
    public const float LOOKSENS_MAX = 10f;
    const int DECIMALPLACES = 2;

    // Value
    public float lookSensitivity;

    // UI Elements
    public TMP_InputField inputField;
    public Slider slider;

    /*
     * Changes a setting in a settings object
     *  Persist settings between scenes for now
     */

    private void Awake()
    {
        // Read current lookSensitivity from PlayerSettings
        ReadCurrentPlayerSettingsSensitivityValue();

        // Slider init
        slider.minValue = LOOKSENS_MIN;
        slider.maxValue = LOOKSENS_MAX;

        // With updated value, update the UI values to match properly
        UpdateUIValues();
    }

    public void SetSensitivityFromInputField()
    {
        // Update sensitivity from input field value
        float inputFloat = 0f;
        if(float.TryParse(inputField.text, out inputFloat))
        {
            UpdateCurrentSensitivityReading(inputFloat);
        }
        else
        {
            throw new System.Exception("Something else except a float value is in the text field: " + inputField.text);
        }

        return;
    }

    public void SetSensitivityFromSlider()
    {
        // Update sensitivity from slider value
        float inputFloat = slider.value;

        UpdateCurrentSensitivityReading(inputFloat);
    }

    protected string RoundToDecimalPlaces(float number, int decimalPlaces)
    { 
        return number.ToString("F" + decimalPlaces);
    }

    protected void UpdateUIValues()
    {
        // Input field
        inputField.SetTextWithoutNotify(lookSensitivity.ToString());

        // Slider; do not trigger callbacks
        slider.SetValueWithoutNotify(lookSensitivity);
    }

    /// <summary>
    /// Processes the raw look sensitivity (clamp, decimal rounding), and updates the player's look sensitivity value.
    /// </summary>
    /// <param name="newLookSensitivity"></param>
    protected void UpdateCurrentSensitivityReading(float newLookSensitivity)
    {
        // Make sure to cut it off to 2 decimal places
        string sensitivityString = RoundToDecimalPlaces(newLookSensitivity, DECIMALPLACES);
        newLookSensitivity = float.Parse(sensitivityString);

        // Clamp within min and max
        //newLookSensitivity = Mathf.Clamp(newLookSensitivity, LOOKSENS_MIN, LOOKSENS_MAX);

        // Update current reading of look sensitivity
        lookSensitivity = newLookSensitivity;

        // Change the look sensitivity in dontdestroyonload
        PlayerSettings.instance.playerSettings.lookSensitivity = newLookSensitivity;

        // Update related UI values
        UpdateUIValues();
    }

    /// <summary>
    /// Read the player settings and update UI values to reflect it.
    /// </summary>
    protected void ReadCurrentPlayerSettingsSensitivityValue()
    {
        lookSensitivity = PlayerSettings.instance.playerSettings.lookSensitivity;
        UpdateUIValues();
    }

    /// <summary>
    /// Remove negative value from an inputfield. Only works for the referenced input field.
    /// </summary>
    public void InputFieldOnValueChanged_RemoveNegatives()
    {
        string text = inputField.text;
        if (text.Length > 0 && text[0] == '-')
            inputField.text = text.Remove(0, 1);
    }
}
