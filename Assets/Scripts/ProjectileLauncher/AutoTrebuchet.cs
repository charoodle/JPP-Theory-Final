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
                yield return new WaitForSeconds(5f);

                Debug.Break();

                Debug.Log("Reloading.");
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
                yield return new WaitForSeconds(5f);

                Debug.Break();

                Debug.Log("Launching.");
                launchButton.InteractWith();
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(HandleReloading());
        yield break;
    }
}
