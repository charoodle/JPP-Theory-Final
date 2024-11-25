using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    protected virtual void Start()
    {
        camera = Camera.main;
    }

    public void Shake(float seconds, float intensity)
    {
        StartCoroutine(ShakeCoroutine(seconds, intensity));
    }

    protected IEnumerator ShakeCoroutine(float seconds, float intensity)
    {
        Quaternion rotation = camera.transform.localRotation;

        Debug.Log("Starting shake coroutine");
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
}
