using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleAnimations : MonoBehaviour
{
    protected Animator animator;
    [SerializeField] ParticleSystem dustCloud;
    [SerializeField] ParticleSystem groundExplosion;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            StartCoroutine(CastleFallToGroundAnim());
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            animator.Play("CastleTouchdown");
        }
    }

    protected IEnumerator CastleFallToGroundAnim()
    {
        animator.Play("CastleFallFromSky");

        yield return new WaitForSeconds(4.95f);

        animator.Play("CastleTouchdown");
    }

    public void PlayDustCloudParticles()
    {
        groundExplosion.Play();
        dustCloud.Play();
    }
}
