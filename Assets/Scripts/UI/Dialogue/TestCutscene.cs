using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCutscene : Cutscene
{
    public TalkWithInteractable henry;

    protected override void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
            TriggerCutscene();

        base.Update();
    }

    protected override IEnumerator CutsceneAction()
    {
        yield return henry.TalkWith();

        yield break;
    }
}
