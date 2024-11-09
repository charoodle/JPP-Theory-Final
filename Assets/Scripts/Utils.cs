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
}
