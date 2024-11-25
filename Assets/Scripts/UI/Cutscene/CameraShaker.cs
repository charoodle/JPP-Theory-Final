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
        Vector3 camLocalPos = camera.transform.localPosition;

        Debug.Log("Starting shake coroutine");
        float timer = 0f;
        while (timer < seconds)
        {
            camera.transform.localPosition = Random.insideUnitSphere * intensity;
            timer += Time.deltaTime;
            yield return null;
        }

        // Revert back to pos
        camera.transform.localPosition = camLocalPos;

        yield break;
    }
}
