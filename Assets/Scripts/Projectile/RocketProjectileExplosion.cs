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

    private void Start()
    {
        explosionDamage = 10f;
        sphereCollider = GetComponent<SphereCollider>();
        StartCoroutine(IncreaseRadius(startRadius, endRadius, duration));
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
