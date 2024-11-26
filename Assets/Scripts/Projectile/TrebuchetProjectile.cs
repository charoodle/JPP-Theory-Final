using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrebuchetProjectile : Projectile
{
    [SerializeField] ParticleSystem particles;
    [SerializeField] ParticleSystem collision_dirtParticles;
    [SerializeField] GameObject embedIntoMaterialPrefab;
    Rigidbody rb;

#if UNITY_EDITOR
    [Tooltip("Ignores forced allowCollisions to false OnEnable.")]
    [SerializeField] bool isDebugProjectile = false;
#endif

    protected override void Start()
    {
        rb = GetComponent<Rigidbody>();
        base.Start();
    }

    protected override void DestroyInAFewSeconds()
    {
        // Do not destroy in a few seconds. Wait for it to be launched instead.
    }

    public void EnableParticles()
    {
        particles.Play();
    }

    public void DisableParticles()
    {
        particles.Stop();
    }

    protected void OnEnable()
    {
#if UNITY_EDITOR
        if (isDebugProjectile)
            return;
#endif
        // Don't allow projectile to be destroyed when its being reloaded
        allowCollisions = false;
    }

    protected void Update()
    {
        // Destroy if projectile infinite falls into void.
        if (transform.position.y < -30f)
            DestroyProjectile();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if (!allowCollisions)
            return;

        // If touch anything, disable flying particles
        DisableParticles();

        // Stop moving. Embed ball within collision point.
        StopProjectileMovement();
        transform.position = collision.GetContact(0).point;

        // Spawn collision particles + embed prefab
        if (collision_dirtParticles)
        {
            // Spawn dirt particles that shoot upwards
            ParticleSystem dirtParticles = Instantiate(collision_dirtParticles, transform.position, collision_dirtParticles.transform.rotation);

            // Spawn embed model to make ball look like its embedded into object
            Vector3 randomYRotation = new Vector3(0f, Random.Range(0f, 360f), 0f);
            GameObject embedObj = Instantiate(embedIntoMaterialPrefab, transform.position, Quaternion.Euler(randomYRotation));

            // Get single color of surface hit
            Color surfaceColor = Color.red + Color.white;
            Renderer surfaceHitRenderer = collision.gameObject.GetComponent<Renderer>();
            RendererMaterialColorChanger colorChanger;
            if (surfaceHitRenderer)
            {
                surfaceColor = MaterialColorGetter.GetColor(surfaceHitRenderer);

                // Change embed model to color of surface it hit
                colorChanger = embedObj.GetComponent<RendererMaterialColorChanger>();
                if (colorChanger)
                {
                    colorChanger.ChangeColor(surfaceColor);
                }

                // Change default start color of dirt particles
                var particleSysMain = dirtParticles.main;
                particleSysMain.startColor = surfaceColor;
            }
        }

        // If hit something with health, then take health away from it.
        if (TakeHealthAwayFrom(collision))
        {
            //DestroyProjectile();
        }

        // Disable any other collisions so it cannot take health away from anything else.
        allowCollisions = false;
    }

    protected override void StopProjectileMovement()
    {
        this.rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
}
