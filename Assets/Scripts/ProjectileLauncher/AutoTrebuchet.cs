using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTrebuchet : Trebuchet
{
    Interactable launchButton;
    Interactable reloadButton;

    protected override void Start()
    {
        base.Start();

        launchButton = trebuchetLaunchPreventer.GetComponent<Interactable>();
        reloadButton = trebuchetReloader.GetComponent<Interactable>();

        // Assumes in launch state
        StartCoroutine(HandleLaunching());
    }

    IEnumerator HandleReloading()
    {
        while(true)
        {
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

    protected override bool IsProjectileReadyToReleaseFromSling(float dotProductOfProjectileToWorldUp, float maxRandomDotProductOffset = 0.04f)
    {
        // Calculate a random point after 1 to release for some randomness
        //float dotOffset = Random.Range(0, maxRandomDotProductOffset);
        return dotProductOfProjectileToWorldUp < 1;
    }
}
