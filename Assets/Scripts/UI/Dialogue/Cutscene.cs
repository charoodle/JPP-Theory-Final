using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Not really used at the moment. See <see cref="TalkWithInteractable" instead./>
/// </summary>
public abstract class Cutscene : MonoBehaviour
{
    public void TriggerCutscene()
    {
        StopAllCoroutines();
        StartCoroutine(StartCutscene());
    }

    protected virtual IEnumerator StartCutscene()
    {
        // Prevents playing a cutscene immediately on Start. Delays a little so all components can do their Start method.
        yield return new WaitForSeconds(0.1f);

        yield return CutsceneAction();
    }

    /// <summary>
    /// The main action of the cutscene should happen here (talking, animations, ...)
    /// </summary>
    /// <returns></returns>
    protected abstract IEnumerator CutsceneAction();

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            CameraShaker.instance.Shake(0.7f, 1f);
        }
    }
}
