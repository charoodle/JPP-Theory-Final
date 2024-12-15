using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleAnimations : MonoBehaviour
{
    protected CastleController castleController;
    protected Animator animator;
    [SerializeField] ParticleSystem dustCloudPrefab;
    [SerializeField] ParticleSystem groundExplosionPrefab;

    private void Start()
    {
        castleController = GetComponentInChildren<CastleController>();
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
            StartCoroutine(CastleTouchdownAnim());
        }
    }

    public IEnumerator CastleFallToGroundAnim()
    {
        // Controller updates character controller move; prevents animations
        castleController.enabled = false;

        animator.Play("CastleFallFromSky");
        float animationLength = 4.95f;
        yield return new WaitForSeconds(animationLength);

        yield return CastleTouchdownAnim();

        // Reenable
        castleController.enabled = true;
        yield break;
    }

    public IEnumerator CastleTouchdownAnim()
    {
        // Castle initial touchdown
        animator.Play("CastleTouchdown");
        CameraShaker.instance.Shake(1.80f, 30f);

        // Aftershock from all the ground debris landing
        yield return new WaitForSeconds(1.90f);
        CameraShaker.instance.Shake(1.5f, 7f);

        yield break;
    }

    public void PlayDustCloudParticles()
    {
        ParticleSystem groundExplosion = Instantiate(groundExplosionPrefab, transform.position, groundExplosionPrefab.transform.rotation);
        groundExplosion.Play();

        ParticleSystem dustCloud = Instantiate(dustCloudPrefab, transform.position, dustCloudPrefab.transform.rotation);
        dustCloud.Play();

    }
}
