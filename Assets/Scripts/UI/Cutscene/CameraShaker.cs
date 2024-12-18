using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShaker : MonoBehaviour
{
    protected static CameraShaker _instance;
    public static CameraShaker instance
    { 
        get
        {
            return _instance;
        }
        protected set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    protected Camera camera;

    protected CinemachineVirtualCamera cinemachineVCam;

    [SerializeField]
    [Range(0f, 1f)]
    float screenShakeMultiplier;

    #region Debug
    [Header("Debug")]
    [SerializeField] private bool shakeScreen_pistol;
    [SerializeField] private bool shakeScreen_rocket;
    [SerializeField] private bool shakeScreen_trebuchet;

    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        // Note: Hardcoded values; values may not be accurate.
        if(shakeScreen_pistol)
        {
            Shake(0.15f, 7f);
            shakeScreen_pistol = false;
        }
        else if(shakeScreen_rocket)
        {
            Shake(0.25f, 15f);
            shakeScreen_rocket = false;
        }
        else if(shakeScreen_trebuchet)
        {
            Shake(2f, 7f);
            shakeScreen_trebuchet = false;
        }

    }
    #endregion

    protected virtual void Start()
    {
        camera = Camera.main;
        cinemachineVCam = GetComponent<CinemachineVirtualCamera>();
    }

    public void Shake(float seconds, float intensity)
    {
        StartCoroutine(ShakeCoroutineCM(seconds, intensity));
    }

    protected IEnumerator ShakeCoroutine(float seconds, float intensity)
    {
        Quaternion rotation = camera.transform.localRotation;

        float timer = 0f;

        float waitTime = 1 / 60f;
        while (timer < seconds)
        {
            camera.transform.localRotation = Quaternion.Euler(Random.insideUnitSphere * intensity);
            timer += waitTime;
            yield return new WaitForSeconds(waitTime);
        }

        // Revert back to pos
        camera.transform.localRotation = rotation;

        yield break;
    }

    /// <summary>
    /// Uses cinemachine. Basic Perlin noise frequency of 0.15 looks good.
    /// </summary>
    protected IEnumerator ShakeCoroutineCM(float seconds, float intensity)
    {
        intensity *= screenShakeMultiplier;

        CinemachineBasicMultiChannelPerlin cinemachineBMCP =
            cinemachineVCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cinemachineBMCP.m_AmplitudeGain = intensity;

        // Make amplitude go from intensity to 0 over seconds.
        float timer = 0f;
        while(timer < seconds)
        {
            float pct = timer / seconds;
            cinemachineBMCP.m_AmplitudeGain = Mathf.Lerp(intensity, 0f, pct);
            timer += Time.deltaTime;
            yield return null;
        }

        cinemachineBMCP.m_AmplitudeGain = 0f;
        yield break;
    }
}
