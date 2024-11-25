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
            StopAllCoroutines();
            StartCoroutine(CastleFallToGroundAnim());
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            StopAllCoroutines();
            StartCoroutine(CastleTouchdownAnim());
        }
    }

    public IEnumerator CastleTouchdownAnim()
    {
        // Castle initial touchdown
        animator.Play("CastleTouchdown");
        CameraShaker.instance.Shake(1.80f, 30f);

        // Aftershock from all the ground debris landing
        yield return new WaitForSeconds(1.90f);
        CameraShaker.instance.Shake(1.5f, 7f);
    }

    public IEnumerator CastleFallToGroundAnim()
    {
        animator.Play("CastleFallFromSky");

        yield return new WaitForSeconds(4.95f);

        yield return CastleTouchdownAnim();
    }

    public void PlayDustCloudParticles()
    {
        groundExplosion.Play();
        dustCloud.Play();
    }
}
