using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkWith_Henry : TalkWithInteractable
{
    protected override IEnumerator TalkWithCoroutine()
    {
        yield return TextBox("Henry", "Hello dear traveler!");

        yield return TextBox("My name is Henry.");

        yield return TextBox("Welcome to my kingdom.");

        yield break;
    }
}
