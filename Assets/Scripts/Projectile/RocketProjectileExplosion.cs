using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectileExplosion : MonoBehaviour
{
    private float _explosionDamage;
    public float explosionDamage
    {
        get { return _explosionDamage; }
        set
        {
            if(value <= 0)
            {
                Debug.LogWarning("Rocket explosion cannot have 0 damage.", this.gameObject);
                value = 1f;
            }
            _explosionDamage = value;
        }
    }
    SphereCollider sphereCollider;
    [SerializeField] float startRadius = 2f;
    [SerializeField] float endRadius = 7f;
    [SerializeField] float duration = 0.4f;
    [SerializeField] float durationUntilDestroyed = 1.15f;

    private void Start()
    {
        explosionDamage = 10f;
        sphereCollider = GetComponent<SphereCollider>();
        StartCoroutine(IncreaseRadius(startRadius, endRadius, duration));

        // Destroy after particles finished && player lands back on ground
        //Destroy(this.gameObject, 1.15f);
    }

    protected IEnumerator IncreaseRadius(float start, float end, float duration)
    {
        float timer = 0f;
        while(timer <= duration)
        {
            timer += Time.deltaTime;

            float pct = timer / duration;
            sphereCollider.radius = Mathf.Lerp(start, end, pct);

            yield return new WaitForFixedUpdate();
        }

        // Destroy sphere collider component when done
        Destroy(sphereCollider);

        yield break;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Damage enemies in radius
        if(IsEnemy(other.gameObject))
        {
            TakeHealthAwayFrom(other.gameObject);
        }

        // Add explosion forces to any rigidbodies in explosion
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if(rb)
        {
            StartCoroutine(HandleRigidbodyExplosion(rb));
        }
    }

    IEnumerator HandleRigidbodyExplosion(Rigidbody rb)
    {
        float timeStarted = Time.time;

        Debug.Log("Adding force to " + rb.gameObject.name);

        // Disable Unity character controller
        CharacterController cc = rb.gameObject.GetComponent<CharacterController>();
        if(cc)
        {
            cc.enabled = false;
        }

        // Use rigidbody to take over for physics
        rb.constraints = RigidbodyConstraints.None;
        rb.AddExplosionForce(10000f, transform.position, sphereCollider.radius, 0f);

        // Wait for character controller to be grounded until give control back
        MyProject.CharacterController myCC = rb.gameObject.GetComponent<MyProject.CharacterController>();
        while(!myCC.isGrounded)
        {
            yield return null;
        }

        Debug.Log("Detecting ground, giving control back.");
        rb.constraints = RigidbodyConstraints.FreezeAll;
        if (cc)
            cc.enabled = true;

        // Player landed - wait until particles are fully finished playing to destroy
        if(Utils.SecondsSince(timeStarted) < durationUntilDestroyed)
        {
            yield return null;
        }
        Destroy(gameObject);

        yield break;
    }

    protected bool IsEnemy(GameObject obj)
    {
        return obj.GetComponent<EnemyController>() != null;
    }

    protected virtual bool TakeHealthAwayFrom(GameObject obj)
    {
        // Take health away from object when hit with this projectile
        //  Colliders usually on child objects. Scripts on parent objects.
        Health health = obj.gameObject.GetComponent<Health>();
        if (health)
        {
            health.TakeDamage(explosionDamage);
            return true;
        }
        return false;
    }
}
