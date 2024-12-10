using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    /// <summary>
    /// Is the object a player?
    /// </summary>
    /// <returns>Returns player controller if it has one. Otherwise will remain null.</returns>
    public static bool IsPlayer(GameObject obj, ref PlayerController player)
    {
        player = obj.GetComponent<PlayerController>();
        return (player != null);
    }

    /// <summary>
    /// Only works with EnemyController.
    /// </summary>
    /// <returns>Returns enemy controller if it has one. Otherwise will remain null.</returns>
    public static bool IsEnemy(GameObject obj, ref EnemyController enemy)
    {
        enemy = obj.GetComponent<EnemyController>();
        return (enemy != null);
    }

    /// <summary>
    /// Is this an enemy that can move around/is human?
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsHumanoidEnemy(GameObject obj)
    {
        return obj.GetComponent<EnemyController>();
    }

    /// <summary>
    /// Detects anything with an EnemyController OR EnemyIdentifier.
    /// </summary>
    public static bool IsEnemy(GameObject obj)
    {
        return (obj.GetComponent<EnemyController>() || obj.GetComponent<EnemyCastle>());
    }

    /// <summary>
    /// How long since a timestamp has passed?
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static float SecondsSince(float time)
    {
        if (time <= 0)
            time = 0f;

        return Time.time - time;
    }
}
