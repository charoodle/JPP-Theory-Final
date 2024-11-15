using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTrebuchet : Trebuchet
{
    Interactable launchButton;
    Interactable reloadButton;
    [SerializeField] EnemyController enemyUser;
    bool enemyUserAlive = false;

    protected override void Start()
    {
        base.Start();

        launchButton = trebuchetLaunchPreventer.GetComponent<Interactable>();
        reloadButton = trebuchetReloader.GetComponent<Interactable>();

        enemyUserAlive = (enemyUser != null);

        // Assumes in launch state
        StartCoroutine(HandleLaunching());

        // If enemy dies, trebuchet stops shooting
        StartCoroutine(CheckIfTrebuchetUserIsDead());
    }

    IEnumerator HandleReloading()
    {
        while(true)
        {
            if (!enemyUserAlive)
                yield break;

            // Reload if reload button is active
            if (trebuchetReloader.activeInHierarchy)
            {
                yield return new WaitForSeconds(3f);
                reloadButton.InteractWith();
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        
        StartCoroutine(HandleLaunching());
        yield break;
    }

    IEnumerator HandleLaunching()
    {
        while (true)
        {
            if (!enemyUserAlive)
                yield break;

            // Launch if launch button is active
            if (trebuchetLaunchPreventer.activeInHierarchy)
            {
                yield return new WaitForSeconds(3f);
                launchButton.InteractWith();
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(HandleReloading());
        yield break;
    }

    IEnumerator CheckIfTrebuchetUserIsDead()
    {
        // If trebuchet user is dead, stop firing
        while(true)
        {
            if(!enemyUser)
            {
                // Stop all firing and launching coroutines
                enemyUserAlive = false;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    protected override bool IsProjectileReadyToReleaseFromSling(float dotProductOfProjectileToWorldUp, float maxRandomDotProductOffset = 0.04f)
    {
        // Calculate a random point after 1 to release for some randomness
        //float dotOffset = Random.Range(0, maxRandomDotProductOffset);
        return dotProductOfProjectileToWorldUp < 1;
    }
}
