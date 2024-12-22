using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportInteractable : UnityEventInteractable
{
    [SerializeField] Transform teleportLocation;

    GameObject player;

    protected override void Start()
    {
        base.Start();

        // TEMP: For title screen, to clear out errors for now (no player in title screen)
        try
        {
            player = FindObjectOfType<PlayerController>().gameObject;
        }
        catch(System.Exception e)
        {

        }
    }

    public void TeleportPlayerToLocation()
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        cc.enabled = false;
        player.transform.position = teleportLocation.position;
        cc.enabled = true;
    }
}
