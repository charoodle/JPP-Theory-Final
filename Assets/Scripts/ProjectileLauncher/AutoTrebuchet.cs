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

        // Titlescreen: Adjust weight randomly between each shot
        counterweightRb.mass += Random.Range(-50f, 50f);
        counterweightRb.mass = Mathf.Clamp(counterweightRb.mass, 230f, 320f);
        // Titlescreen: Delay a random amt of seconds before next shot so not all active trebuchets shots are desync'd
        float sec = Random.Range(2f, 7f);
        yield return new WaitForSeconds(sec);

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

    protected override void ShakeCamera()
    {
        // TEMP - Don't shake camera for auto trebuchet.
        //  Temporary solution for title screen auto trebuchet.
        //  Currentl only player's trebuchet should shake screen
        //  TODO: Make a better solution: check if player closer enough to shake their camera
        return;
    }
}
