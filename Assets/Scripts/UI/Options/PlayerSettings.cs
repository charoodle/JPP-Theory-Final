using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Persist player's settings (ex: sensitivity) throughout scene changes.
/// Updates player controller (if exists) if there is an update to its sensitivity value.
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
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    /// <summary>
    /// Mouse sensitivity.
    /// </summary>
    public float lookSensitivity
    {
        get { return _lookSensitivity; }
        set 
        {
            Debug.Log("Setting look sensitivity to " + value);

            _lookSensitivity = value;

            // TODO: Update player controller settings
            PlayerController player = FindObjectOfType<PlayerController>();
            if(player)
            {
                player.lookXSens = _lookSensitivity;
                player.lookYSens = _lookSensitivity;
            }
        }
    }

    private static PlayerSettings _instance;
    /// Note: Player Settings should be initialized on game start, so default value here is just temporary until can figure out persisting data between game sessions.
    ///     so it doesnt start me off at the minimum sensitivity each time.
    private float _lookSensitivity = SensitivitySetting.LOOKSENS_DEFAULT;

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
    }
}
