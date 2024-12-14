using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Version of Healthbar where it reppears with full health after it dies.
/// 
/// Make sure this object does not have any mesh renderers. On death, makes all children disappear.
/// Model + hitboxes should be on children.
/// 
/// Good for target practice, especially in tutorial.
/// </summary>
public class HealthRespawnable : Health
{
    [SerializeField] float respawnSeconds = 5f;



    protected override void DestroyObject()
    {
        // Respawns in few seconds instead of being destroyed
        StopAllCoroutines();
        StartCoroutine(RespawnInSeconds(respawnSeconds));
    }

    ///<summary>Make entire object disappear and then reappear with full health - assuming children has all the visuals + hitboxes.</summary> 
    protected virtual IEnumerator RespawnInSeconds(float seconds)
    {
        MakeAllChildrenActive(false);

        yield return new WaitForSeconds(seconds);

        MakeAllChildrenActive(true);
        RefillHealthToFull();

        yield break;
    }

    protected void MakeAllChildrenActive(bool active)
    {
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(active);
        }
    }
}
