using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Launches one ammo projectile at a time.
/// </summary>
public abstract class ProjectileLauncher : MonoBehaviour
{
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
            if (reloadProjectileCoroutine_CanStopEarly)
                ReloadProjectileCoroutine_StopEarly();

            return reloadCoroutine != null;
        }
    }

    /// <summary>
    /// How many projectiles get refilled per reload cycle.
    /// </summary>
    protected int ammoToRefillPerReload
    {
        get
        {
            return _ammoToRefillPerReload;
        }
        set
        {
            if (value <= 0)
                Debug.LogError($"Ammo to refill per reload cannot be <= 0. value={value}");
            _ammoToRefillPerReload = value;
        }
    }

    /// <summary>
    /// (Backing Field + Inspector Field) The projectile prefab to be launched.
    /// </summary>
    [SerializeField] protected GameObject _projectilePrefab;

    /// <summary>
    /// (Backing Field) How much ammo to refill per reload.
    /// </summary>
    protected int _ammoToRefillPerReload;

    [SerializeField] protected Transform shootPoint;

    [SerializeField] protected TextMeshProUGUI reloadingText;

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
    /// How long the UI pops up "Done reloading!" after reloading.
    /// </summary>
    protected float doneReloadPopupTime = 0.5f;

    /// <summary>
    /// Can the ReloadProjectileCoroutine be stopped early?
    /// </summary>
    protected bool reloadProjectileCoroutine_CanStopEarly = false;
    


    protected virtual void Start()
    {
        GunUICanvasEnabled(false);
        ReloadProjectile();
    }

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
        reloadProjectileCoroutine_CanStopEarly = false;

        // Reload Gun UI Canvas turn on
        GunUICanvasEnabled(true);
        GunUICanvasSetText("Reloading...");

        // Customized reload delay - wait for different amount of time/different procedures.
        yield return ReloadProjectileCoroutine_WaitForSeconds();

        // Customized reload amount for each projectile launcher.
        yield return ReloadProjectileCoroutine_RefillAmmunitionCount(ammoToRefillPerReload);

        // Allowed to skip the rest of this coroutine, and skip into the next shoot/reload cycle.
        reloadProjectileCoroutine_CanStopEarly = true;

        // Pop up that it's done reloading briefly
        yield return ReloadProjectileCoroutine_DoneReloadTextPopup(doneReloadPopupTime);

        // Null out coroutine for next use
        reloadCoroutine = null;
        // No coroutine, so nothing to stop early
        reloadProjectileCoroutine_CanStopEarly = false;

        yield break;
    }

    protected virtual IEnumerator ReloadProjectileCoroutine_DoneReloadTextPopup(float seconds)
    {
        GunUICanvasSetText("Done reloading!");
        yield return new WaitForSeconds(seconds);
        // Reload Gun UI Canvas Turn off
        GunUICanvasEnabled(false);
    }

    /// <summary>
    /// Customize your own reload coroutine here (ex: delay wait for seconds, play a reload animation, ...)
    /// </summary>
    protected abstract IEnumerator ReloadProjectileCoroutine_WaitForSeconds();

    /// <summary>
    /// Customize how much ammo gets reloaded here per reload.
    /// </summary>
    protected virtual IEnumerator ReloadProjectileCoroutine_RefillAmmunitionCount(int ammoToRefill)
    {
        ammoCount = ammoToRefill;
        yield break;
    }

    /// <summary>
    /// Force the reload coroutine to stop early if it's running.
    /// </summary>
    protected void ReloadProjectileCoroutine_StopEarly()
    {
        if (reloadCoroutine == null)
            return;

        // Turn off Gun Canvas if stopping reload early - player wants to shoot
        GunUICanvasEnabled(false);

        StopCoroutine(reloadCoroutine);
        reloadCoroutine = null;
    }

    /// Spawn a projectile at location with rotation.
    protected virtual GameObject SpawnProjectile(Vector3 location, Quaternion rotation)
    {
        return Instantiate(projectilePrefab, location, rotation);
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
        reloadingText.gameObject.SetActive(enabled);
    }

    protected virtual void GunUICanvasSetText(string text)
    {
        reloadingText.SetText(text);
    }
}