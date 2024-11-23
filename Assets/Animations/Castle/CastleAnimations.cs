using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleAnimations : MonoBehaviour
{
    protected Animator animator;
    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            animator.Play("CastleFallFromSky");
        }
    }

    public void PlayDustCloudParticles()
    {

    }
}
