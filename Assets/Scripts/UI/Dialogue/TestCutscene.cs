using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCutscene : Cutscene
{
    public TalkWithInteractable henry;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            TriggerCutscene();
    }

    protected override IEnumerator CutsceneAction()
    {
        yield return henry.TalkWith();

        yield break;
    }
}
