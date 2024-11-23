using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCutscene : Cutscene
{
    public TalkWithInteractable person;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            TriggerCutscene();
    }

    protected override IEnumerator CutsceneAction()
    {
        person.TalkWith();
        yield break;
    }
}
