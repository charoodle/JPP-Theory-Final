using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonDataServiceTest : MonoBehaviour
{
    public bool savePlayerSettings;
    public bool loadPlayerSettings;
    public bool deletePlayerSettings_file;
    public bool saveWithPrettyIndent = false;

    [Header("Current Settings")]
    public bool clearPlayerSettings_inspector;
    public PlayerSettings1 currentPlayerSettings;

    // Relative File Path
    protected const string playerSettingsFilePath = "/player_settings.json";

    protected const string separator = "--------------------------";



    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        if(savePlayerSettings)
        {
            savePlayerSettings = false;
            SavePlayerSettings();
        }
        else if (loadPlayerSettings)
        {
            loadPlayerSettings = false;
            LoadPlayerSettings();
        }
        else if(clearPlayerSettings_inspector)
        {
            clearPlayerSettings_inspector = false;
            currentPlayerSettings = new PlayerSettings1();
        }
        else if (deletePlayerSettings_file)
        {
            deletePlayerSettings_file = false;
            DeletePlayerSettings_File();
        }
    }



    protected void SavePlayerSettings()
    {
        Debug.Log("Saving player settings...");
        string path = JsonDataService.Save(currentPlayerSettings, playerSettingsFilePath, saveWithPrettyIndent);
        Debug.Log($"Successully saved player settings to: {path}.");
        Debug.Log(separator);
    }

    protected void LoadPlayerSettings()
    {
        Debug.Log("Loading player settings...");
        currentPlayerSettings = JsonDataService.Load<PlayerSettings1>(playerSettingsFilePath);
        // TODO: Validate each setting when loading in
        Debug.Log("Successfully loaded player settings!");
        Debug.Log(separator);
    }

    protected void DeletePlayerSettings_File()
    {
        Debug.Log("Deleting player settings file...");
        JsonDataService.DeleteFile(playerSettingsFilePath);
        Debug.Log("Successfully deleted player settings file.");
        Debug.Log(separator);
    }




    [System.Serializable]
    public class PlayerSettings1
    {
        public float lookSensitivity;
        public float cameraShake;
    }
}
