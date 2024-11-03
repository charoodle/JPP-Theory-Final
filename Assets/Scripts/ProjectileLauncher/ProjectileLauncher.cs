using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public abstract class ProjectileLauncher : MonoBehaviour
{
    protected void Start()
    {
        GunUICanvasEnabled(false);
        ReloadProjectile();
    }

    /// <summary>
    /// The projectile prefab to be launched out.
    /// </summary>
    public GameObject projectilePrefab
    {
        get
        {
            return _projectilePrefab;
        }
        protected set
        {
            _projectilePrefab = value;
        }
    }

    /// <summary>
    /// Is the launcher reloading right now?
    /// </summary>
    protected bool isReloading
    {
        get
        {
            return reloadCoroutine != null;
        }
    }

    /// <summary>
    /// The projectile prefab to be launched.
    /// </summary>
    [SerializeField] protected GameObject _projectilePrefab;

    [SerializeField] protected TextMeshProUGUI _reloadingText;
    
    /// <summary>
    /// How much force the projectile should be launched with.
    /// </summary>
    [SerializeField] protected float launchForce;

    protected Coroutine reloadCoroutine;

    public abstract void LaunchProjectile();

    public virtual void ReloadProjectile()
    {
        // Do not reload again if coroutine already running
        if (isReloading)
            return;

        reloadCoroutine = StartCoroutine(ReloadProjectileCoroutine());
    }

    protected virtual IEnumerator ReloadProjectileCoroutine()
    {
        // Reload Gun UI Canvas turn on
        GunUICanvasEnabled(true);
        _reloadingText.SetText("Reloading...");

        // Customized reload delay - wait for different amount of time/different procedures.
        yield return ReloadProjectileCoroutine_WaitForSeconds();

        // Customized reload amount for each projectile launcher.
        yield return ReloadProjectileCoroutine_RefillAmmunitionCount();

        // Reload Gun UI Canvas Turn off
        GunUICanvasEnabled(false);

        // Null out coroutine for next use
        reloadCoroutine = null;
        yield break;
    }

    /// <summary>
    /// Customize your own reload coroutine here.
    /// </summary>
    protected abstract IEnumerator ReloadProjectileCoroutine_WaitForSeconds();

    /// <summary>
    /// Customize how much ammo gets reloaded here. You manage your own ammo counts.
    /// </summary>
    protected abstract IEnumerator ReloadProjectileCoroutine_RefillAmmunitionCount();

    /// Spawn a projectile at location with rotation.
    protected virtual GameObject SpawnProjectile(Vector3 location, Quaternion rotation)
    {
        return Instantiate(projectilePrefab, location, rotation);
    }

    /// Spawn a projectile at transform's position, matching forward dir with transform's forward dir rotation.
    protected virtual GameObject SpawnProjectile(Transform transform)
    {
        return Instantiate(projectilePrefab, transform.position, transform.rotation);
    }

    /// <summary>
    /// Show status of gun while in-game (ex: reloading).
    /// </summary>
    protected virtual void GunUICanvasEnabled(bool enabled)
    {
        // Reload Gun UI Canvas
        _reloadingText.gameObject.SetActive(enabled);
    }
}
