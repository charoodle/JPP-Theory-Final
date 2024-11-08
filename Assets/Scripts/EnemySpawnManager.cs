using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] protected GameObject[] enemyPrefabs;
    [SerializeField] protected BoxCollider spawnBox;

    protected void Start()
    {
        InvokeRepeating("SpawnEnemy", 3.0f, 2.0f);
    }

    protected void SpawnEnemy()
    {
        Vector3 randomPos = GetRandomPosInBox(spawnBox);
        SpawnRandomEnemy(randomPos, spawnBox.transform.rotation);
    }

    protected GameObject SpawnRandomEnemy(Vector3 worldPos, Quaternion rotation)
    {
        int rand = Random.Range(0, enemyPrefabs.Length);
        return Instantiate(enemyPrefabs[rand], worldPos, rotation);
    }

    protected static Vector3 GetRandomPosInBox(BoxCollider box)
    {
        if (!box.enabled)
            Debug.LogWarning("Box not enabled. Its bounds values may not be initialized.");

        Vector3 pos = new Vector3(Random.Range(box.bounds.min.x, box.bounds.max.x),
                                  Random.Range(box.bounds.min.y, box.bounds.max.y),
                                  Random.Range(box.bounds.min.z, box.bounds.max.z) );
        return pos;
    }
}
