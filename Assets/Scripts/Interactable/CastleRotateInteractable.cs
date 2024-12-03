using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleRotateInteractable : CastleMoveInteractable
{
    public override void InteractWith()
    {
        if (!canMove)
            return;

        castleCtrl.TurnCastle(moveDir);
    }
}
