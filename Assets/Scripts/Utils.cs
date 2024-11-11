using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static bool IsEnemy(GameObject obj, ref EnemyController enemy)
    {
        enemy = obj.GetComponent<EnemyController>();
        return (enemy != null);
    }

    public static bool IsEnemy(GameObject obj)
    {
        return (obj.GetComponent<EnemyController>());
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
