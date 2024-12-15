using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnightAnimations : MonoBehaviour
{
    [SerializeField] Animator animator;

    [Header("Debug")]
    [SerializeField] bool idle;
    [SerializeField] bool walk;
    [SerializeField] bool run;

    private void OnValidate()
    {
        if(idle)
        {
            idle = false;
            Idle();
        }

        if(walk)
        {
            walk = false;
            Walk();
        }

        if(run)
        {
            run = false;
            Run();
        }
    }

    public void Idle()
    {
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
    }

    public void Walk()
    {
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsRunning", false);
    }

    public void Run()
    {
        animator.SetBool("IsRunning", true);
        animator.SetBool("IsWalking", false);
    }
}
