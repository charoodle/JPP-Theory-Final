using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// Persist player's settings (ex: sensitivity) throughout scene changes and game sessions.
/// Updates player controller (if exists) if there is an update to its sensitivity value.
/// 
/// Can look at current values using the Inspector's "Debug" toggle to view private fields.
/// </summary>
public class PlayerSettings : MonoBehaviour
{   
    public static PlayerSettings instance
    {
        get
        {
            // No instance in scene/DontDestroyOnLoad yet; Create new singleton and assign + return it
            if (_instance == null)
            {
                GameObject newGameObj = new GameObject("PlayerSettings");
                PlayerSettings playerSettings = newGameObj.AddComponent<PlayerSettings>();
                _instance = playerSettings;

                // TODO: Make it load player settings from file here.
                //playerSettings.LoadCurrentValuesFromFile();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    /// <summary>
    /// All the player's settings that will show up in their options menu.
    /// </summary>
    public PlayerSettingsData playerSettings;

    /// <summary>
    /// Represents the player's options menu data.
    /// </summary>
    [System.Serializable]
    public class PlayerSettingsData
    {
        // To let options menu determine if player changed a setting or not. Manually call for each changed setting below.
        public delegate void SettingChangedAction();
        [JsonIgnore]
        public SettingChangedAction OnAnySettingChanged;

        /// <summary>
        /// Mouse sensitivity.
        /// </summary>
        public float lookSensitivity
        {
            get { return _lookSensitivity; }
            set
            {
                // No changes
                if (_lookSensitivity == value)
                    return;

                OnAnySettingChanged?.Invoke();
                _lookSensitivity = value;

                // TODO: Update player controller settings
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player)
                {
                    player.lookXSens = _lookSensitivity;
                    player.lookYSens = _lookSensitivity;
                }
            }
        }

        /// <summary>
        /// Camera shake intensity.
        /// </summary>
        public float cameraShakeIntensity
        {
            get { return _cameraShakeIntensity; }
            set
            {
                if (_cameraShakeIntensity == value)
                    return;

                OnAnySettingChanged?.Invoke();
                _cameraShakeIntensity = value;

                // TODO: Update camera shake settings
                if(CameraShaker.instance)
                    CameraShaker.instance.screenShakeMultiplier = _cameraShakeIntensity;
            }
        }

        /// Note: Player Settings should be initialized on game start, so default value here is just temporary until can figure out persisting data between game sessions.
        ///     so it doesnt start me off at the minimum sensitivity each time.
        /// TODO: Separate default values list stored somewhere if loading from file fails? Do not keep here?
        private float _lookSensitivity = SensitivitySetting.LOOKSENS_DEFAULT;
        private float _cameraShakeIntensity = CAMSHAKE_INTENSITY_DEFAULT;

        private const float CAMSHAKE_INTENSITY_DEFAULT = 1.0f;
    }

    /// <summary> Uses the persistent data path </summary>
    private const string relativeFilePath = "/player-settings.json";
    private static PlayerSettings _instance;

    private void Awake()
    {
        // Destroy any new PlayerSettings that isn't this instance; singleton
        if(_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            // No instance; assign + DDOL
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        LoadCurrentValuesFromFile();
    }

    [SerializeField] bool saveSettings;
    private void OnValidate()
    {
        // Debug: Manual save (inspector button)
        if(saveSettings)
        {
            saveSettings = false;
            SaveCurrentValuesToFile();
        }
    }



    public bool SaveCurrentValuesToFile()
    {
#if UNITY_WEBGL
        // TODO: Test WebGL for persistent data?
        return;
#endif

        Debug.Log("Saving...");
        bool success = JsonDataService.Save(playerSettings, relativeFilePath, true);
        if(success)
            Debug.Log("Save successful!");
        return success;
    }

    public void LoadCurrentValuesFromFile()
    {
#if UNITY_WEBGL
        // Uses default values.
        return;
#endif

        try
        {
            Helper_LoadSettingsFromDefaultPath();

            // TODO?: Update all things that use those settings, if they exist
        }
        catch(System.Exception e)
        {
            // If file doesn't exist still, try to create a new one with default values.
            //  Happened when debugging in-editor.

            Debug.LogWarning("Save file doesn't exist, creating a new one...");

            // Try to save and load default settings values.
            try
            {
                // Create PlayerSettingsData with default values
                PlayerSettingsData defaultSettings = new PlayerSettingsData();
                playerSettings = defaultSettings;

                // Save that as a new file
                SaveCurrentValuesToFile();
                Helper_LoadSettingsFromDefaultPath();
            }
            catch (System.Exception f)
            {
                // Can't save/load.
                // Use default settings
                Debug.LogError("Cannot save/load player settings successfully. Using default player settings values. See exception on next line:");

                // TODO: No idea what this could possibly say. Out of storage space on drive? Idk.
                //  If no storage space... how can they install the game? Should include the file within the download "budget"?
                throw f;
            }
        }
    }

    private void Helper_LoadSettingsFromDefaultPath()
    {
        // Load values from file
        Debug.Log("Loading...");
        playerSettings = JsonDataService.Load<PlayerSettingsData>(relativeFilePath);
        Debug.Log("Loading successful!");
    }
}
