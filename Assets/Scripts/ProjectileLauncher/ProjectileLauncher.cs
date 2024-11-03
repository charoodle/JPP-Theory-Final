using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Launches one ammo projectile at a time.
/// </summary>
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
            // Stop coroutine if coroutine says it can be stopped early (ex: early shoot/reload)
            if (ReloadProjectileCoroutine_CanStopEarly)
                ReloadProjectileCoroutine_StopEarly();

            return reloadCoroutine != null;
        }
    }

    /// <summary>
    /// The projectile prefab to be launched.
    /// </summary>
    [SerializeField] protected GameObject _projectilePrefab;

    [SerializeField] protected Transform shootPoint;

    [SerializeField] protected TextMeshProUGUI _reloadingText;

    /// <summary>
    /// How much force the projectile should be launched with.
    /// </summary>
    [SerializeField] protected float launchForce;

    /// <summary>
    /// How much ammo that is currently loaded in the magazine.
    /// </summary>
    [SerializeField] protected int ammoCount;

    protected Coroutine reloadCoroutine;

    /// <summary>
    /// Can the ReloadProjectileCoroutine be stopped early?
    /// </summary>
    protected bool ReloadProjectileCoroutine_CanStopEarly = false;


    protected void Update()
    {
        if (CheckForLaunchInput())
            LaunchProjectile();
    }

    /// <summary>
    /// If true, launches the projectile.
    /// </summary>
    protected abstract bool CheckForLaunchInput();

    public virtual void LaunchProjectile()
    {
        // Out of ammo
        if (ammoCount <= 0)
        {
            // Start reload instead
            ReloadProjectile();
            return;
        }

        // Is currently reloading
        if (isReloading)
        {
            return;
        }

        // Subtract 1 ammo
        ammoCount--;

        // Spawn projectile at barrel, facing same forward dir as barrel
        Projectile projectile = SpawnProjectile(shootPoint);

        // Launch projectile forward with force
        LaunchProjectile_Forwards(projectile, launchForce);
    }

    protected abstract void LaunchProjectile_Forwards(Projectile projectile, float launchForce);

    public virtual void ReloadProjectile()
    {
        // Do not reload again if coroutine already running
        if (isReloading)
        {
            return;
        }

        reloadCoroutine = StartCoroutine(ReloadProjectileCoroutine());
    }

    protected virtual IEnumerator ReloadProjectileCoroutine()
    {
        // Cannot skip this reloading part.
        ReloadProjectileCoroutine_CanStopEarly = false;

        // Reload Gun UI Canvas turn on
        GunUICanvasEnabled(true);
        _reloadingText.SetText("Reloading...");

        // Customized reload delay - wait for different amount of time/different procedures.
        yield return ReloadProjectileCoroutine_WaitForSeconds();

        // Customized reload amount for each projectile launcher.
        yield return ReloadProjectileCoroutine_RefillAmmunitionCount();

        // Allowed to skip the rest of this coroutine, and skip into the next shoot/reload cycle.
        ReloadProjectileCoroutine_CanStopEarly = true;
        
        // Pop up that it's done reloading briefly
        _reloadingText.SetText("Done reloading!");
        yield return new WaitForSeconds(0.5f);
        // Reload Gun UI Canvas Turn off
        GunUICanvasEnabled(false);

        // Null out coroutine for next use
        reloadCoroutine = null;
        // No coroutine, so nothing to stop early
        ReloadProjectileCoroutine_CanStopEarly = false;

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

    /// <summary>
    /// Force the reload coroutine to stop early if it's running.
    /// </summary>
    protected void ReloadProjectileCoroutine_StopEarly()
    {
        if (reloadCoroutine == null)
            return;

        StopCoroutine(reloadCoroutine);
        reloadCoroutine = null;
    }

    /// Spawn a projectile at transform's position, matching forward dir with transform's forward dir rotation.
    protected virtual Projectile SpawnProjectile(Transform transform)
    {
        return Instantiate(projectilePrefab, transform.position, transform.rotation).GetComponent<Projectile>();
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
