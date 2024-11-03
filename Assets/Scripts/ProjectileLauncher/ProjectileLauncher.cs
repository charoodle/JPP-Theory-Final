using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileLauncher : MonoBehaviour
{
    /// <summary>
    /// The projectile prefab to be launched.
    /// </summary>
    [SerializeField] protected GameObject _projectile;
    public GameObject projectile
    {
        get
        {
            return _projectile;
        }
        protected set
        {
            _projectile = value;
        }
    }

    /// <summary>
    /// How much force the projectile should be launched with.
    /// </summary>
    [SerializeField] protected float launchForce;

    public abstract void LaunchProjectile();

    public abstract void ReloadProjectile();

    /// Spawn a projectile at location with rotation.
    protected virtual GameObject SpawnProjectile(Vector3 location, Quaternion rotation)
    {
        return Instantiate(projectile, location, rotation);
    }

    /// Spawn a projectile at transform's position, matching forward dir with transform's forward dir rotation.
    protected virtual GameObject SpawnProjectile(Transform transform)
    {
        return Instantiate(projectile, transform.position, transform.rotation);
    }
}
