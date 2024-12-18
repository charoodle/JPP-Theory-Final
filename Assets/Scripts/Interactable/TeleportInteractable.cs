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

        player = FindObjectOfType<PlayerController>().gameObject;
    }

    public void TeleportPlayerToLocation()
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        cc.enabled = false;
        player.transform.position = teleportLocation.position;
        cc.enabled = true;
    }
}
