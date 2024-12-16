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

    /// <summary>
    /// Track which enemies already received damage from explosion. Prevent doubled up damage from same explosion.
    /// </summary>
    List<GameObject> enemiesDamaged;

    private void Start()
    {
        explosionDamage = 10f;
        sphereCollider = GetComponent<SphereCollider>();
        StartCoroutine(IncreaseRadius(startRadius, endRadius, duration));

        // Destroy after particles finished
        Destroy(this.gameObject, 1.15f);

        // Init list
        enemiesDamaged = new List<GameObject>();
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
        // Damage enemies (enemies or castles) in radius once. Check the object that has the health component to determine if already did damage
        Health health = other.GetComponent<Health>();
        if (!health)
            return;

        GameObject enemy = health.gameObject;
        if(Utils.IsEnemy(enemy) && !AlreadyDidDamageTo(enemy))
        {
            TakeHealthAwayFrom(enemy);
        }

        // Add explosion forces to any rigidbodies in explosion
        //Rigidbody rb = other.GetComponent<Rigidbody>();
        //if(rb)
        //{
        //    StartCoroutine(HandleRigidbodyExplosion(rb));
        //}
    }

    private bool AlreadyDidDamageTo(GameObject enemy)
    {
        return enemiesDamaged.Contains(enemy.transform.root.gameObject);
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

    protected virtual bool TakeHealthAwayFrom(GameObject obj)
    {
        // Take health away from object when hit with this projectile
        //  Colliders usually on child objects. Scripts on parent objects.
        Health health = obj.GetComponent<Health>();
        if (health)
        {
            health.TakeDamage(explosionDamage);
            enemiesDamaged.Add(obj.gameObject);
            return true;
        }

        return false;
    }
}
