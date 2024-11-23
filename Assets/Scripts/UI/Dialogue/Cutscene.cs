using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public TalkWithInteractable person;

    private void Start()
    {
        
    }

    public void TriggerCutscene()
    {
        StopAllCoroutines();
        StartCoroutine(StartCutscene());
    }

    protected IEnumerator StartCutscene()
    {
        yield return new WaitForSeconds(0.1f);
        person.TalkWith();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            TriggerCutscene();
    }
}
