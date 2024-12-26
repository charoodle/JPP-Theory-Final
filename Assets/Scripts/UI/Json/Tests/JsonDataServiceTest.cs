using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonDataServiceTest : MonoBehaviour
{
    public bool savePlayerSettings;
    public bool clearPlayerSettings_inspector;
    public bool loadPlayerSettings;
    public bool clearPlayerSettings_file;
    public PlayerSettings1 currentPlayerSettings;

    // Relative File Path
    protected const string playerSettingsFilePath = "/player_settings.json";



    private void OnValidate()
    {
        if(savePlayerSettings && Application.isPlaying)
        {
            savePlayerSettings = false;
            SavePlayerSettings();
        }
        else if(clearPlayerSettings_inspector)
        {
            clearPlayerSettings_inspector = false;
            currentPlayerSettings = new PlayerSettings1();
        }
        //else if(clearPlayerSettings_file)
        //{
        //    JsonDataService.DeleteFile("")
        //}
    }

    protected void SavePlayerSettings()
    {
        Debug.Log("Saving player settings...");
        string path = JsonDataService.Save(currentPlayerSettings, playerSettingsFilePath, prettyIndent:true);
        Debug.Log($"Successully saved player settings to: {path}.");
    }

    protected void LoadPlayerSettings()
    {
        //Debug.Log("Loading player settings...");
        //currentPlayerSettings = JsonDataService.Load<PlayerSettings1>(playerSettingsFilePath);
        //Debug.Log("Successfully loaded player settings!");
    }

    protected void ClearPlayerSettings_File()
    {

    }




    [System.Serializable]
    public class PlayerSettings1
    {
        public float lookSensitivity;
        public float cameraShake;
    }
}
