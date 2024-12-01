using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleMoveInteractable : Interactable
{
    public CastleController.MoveDir moveDir;
    protected CastleController castleCtrl;

    /// <summary>
    /// Can this button be used to move the castle?
    /// </summary>
    protected bool canMove = true;

    /// <summary>
    /// The interact text on start.
    /// </summary>
    protected string interactTextOrig;

    public override void InteractWith()
    {
        if (!canMove)
            return;

        castleCtrl.MoveCastle(moveDir);
    }

    protected void Awake()
    {
        castleCtrl = GetComponentInParent<CastleController>();
    }

    private void OnEnable()
    {
        // Save a copy of original interact text
        interactTextOrig = interactText;

        // Subscribe to when castle starts/stops moving
        castleCtrl.OnStartMove += OnCastleStartMove;
        castleCtrl.OnStopMove  += OnCastleStopMove;
    }

    private void OnDisable()
    {
        if(castleCtrl)
        {
            castleCtrl.OnStartMove -= OnCastleStartMove;
            castleCtrl.OnStopMove  -= OnCastleStopMove;
        }
    }


    protected void OnCastleStartMove()
    {
        // Castle can only do one move at a time
        canMove = false;
        // Tell player button is unavaiable
        interactText += "\n(Castle is currently moving.)";
    }

    protected void OnCastleStopMove()
    {
        // Castle can move again
        canMove = true;
        // Reset to original interact text
        interactText = interactTextOrig;
    }
}
