using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleAnimations : MonoBehaviour
{
    protected CastleController castleController;
    protected Animator animator;
    [SerializeField] ParticleSystem dustCloudPrefab;
    [SerializeField] ParticleSystem groundExplosionPrefab;

    /// <summary>
    /// Makes castle appear connected to ground; esp for uneven terrain.
    /// </summary>
    [SerializeField] GameObject castleBase;

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

    /// <summary>
    /// Make the castle meshes appear/disappear
    /// </summary>
    /// <param name="appear"></param>
    public void Appear(bool appear)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = appear;
    }

    /// <summary>
    /// Show/hide UI elements of the castle.
    /// </summary>
    public void AppearUI(bool appear)
    {
        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach (Canvas c in canvases)
            c.enabled = appear;
    }

    /// <summary>
    /// Show/hide base that lets castle appear connected to ground.
    /// </summary>
    /// <param name="appear"></param>
    public void AppearCastleBase(bool appear)
    {
        castleBase.GetComponent<Renderer>().enabled = appear;
    }

    public IEnumerator CastleFallToGroundAnim()
    {
        // Controller updates character controller move; prevents animations
        castleController.enabled = false;

        // Play animation
        animator.Play("CastleFallFromSky");

        // Make only castle appear; hide UI and base
        Appear(true);
        AppearCastleBase(false);
        AppearUI(false);

        // Wait for castle to fall from sky
        float animationLength = 4.95f;
        yield return new WaitForSeconds(animationLength);

        // Now reenable UI and base
        AppearCastleBase(true);
        AppearUI(true);

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
