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
    private int _ammoToRefillPerReload;

    [SerializeField] protected Transform shootPoint;

    [SerializeField] protected TextMeshProUGUI reloadingText;

    /// <summary>
    /// How much force the projectile should be launched with.
    /// </summary>
    [SerializeField] protected float launchForce;

    /// <summary>
    /// How long it should take to reload the launcher.
    /// </summary>
    [SerializeField] protected float reloadSeconds = 3f;

    /// <summary>
    /// How much ammo that is currently loaded in the magazine.
    /// </summary>
    protected int ammoCount
    {
        get
        {
            return _ammoCount;
        }
        set
        {
            if(value < 0)
            {
                Debug.LogError($"Cannot set ammoCount to < 0. value={value}");
                return;
            }

            _ammoCount = value;
        }
    }

    [SerializeField] protected int _ammoCount;

    protected Coroutine reloadCoroutine;

    /// <summary>
    /// How long the UI pops up "Done reloading!" after reloading.
    /// </summary>
    private float doneReloadPopupTime = 0.5f;

    /// <summary>
    /// Can the ReloadProjectileCoroutine be stopped early?
    /// </summary>
    protected bool reloadProjectileCoroutine_CanStopEarly = false;

    /// <summary>
    /// Has the derived class initialized this launcher with appropriate values?
    /// </summary>
    protected bool isInit;

    protected MeshRenderer[] weaponModels;

    /// <summary>
    /// Show and hide the weapon from view. Monobehavior is enabled/disabled to allow coroutines to run in the background (ex reloading).
    /// </summary>
    public virtual void EquipWeapon(bool wantToEquip)
    {
        if(weaponModels == null)
             weaponModels = GetComponentsInChildren<MeshRenderer>();

        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        ShowWeapon(wantToEquip);
        this.enabled = wantToEquip;
    }

    /// <summary>
    /// Enable/disable the weapon's mesh renderers.
    /// </summary>
    protected virtual void ShowWeapon(bool shown)
    {
        if(weaponModels == null)
        {
            Debug.LogError("No mesh renderers on weapon. Cannot show weapon.", gameObject);
            return;
        }

        // Enable/disable all mesh renderers
        if (weaponModels.Length >= 1)
        {
            foreach(MeshRenderer weaponModel in weaponModels)
            {
                weaponModel.enabled = shown;
            }
        }

        // Enable/disable all canvases
        Canvas[] canvases = GetComponentsInChildren<Canvas>();
        foreach(Canvas canvas in canvases)
        {
            canvas.enabled = shown;
        }
    }

    protected virtual void Init(float doneReloadPopupTime = 0.5f, int ammoToRefillPerReload = 1)
    {
        this.doneReloadPopupTime = doneReloadPopupTime;
        this.ammoToRefillPerReload = ammoToRefillPerReload;
        isInit = true;
    }

    protected virtual void Start()
    {
        Init();
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
        // Make sure can launch projectile
        if (!LaunchProjectile_Validation())
            return;

        // Subtract 1 ammo
        ammoCount--;

        // Spawn projectile at barrel, facing same forward dir as barrel
        Projectile projectile = SpawnProjectile(shootPoint);

        // Launch projectile forward with force
        LaunchProjectile_Forwards(projectile, launchForce);
    }

    protected virtual bool LaunchProjectile_Validation()
    {
        // Potential warning
        if (!isInit)
            Debug.LogWarning("ProjectileLauncher is not init.", gameObject);

        // Out of ammo - start reload instead
        if (ammoCount <= 0)
        {
            ReloadProjectile();
            return false;
        }

        // Is currently reloading
        if (isReloading)
        {
            return false;
        }

        return true;
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
    protected virtual IEnumerator ReloadProjectileCoroutine_WaitForSeconds()
    {
        yield return new WaitForSeconds(reloadSeconds);
    }

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