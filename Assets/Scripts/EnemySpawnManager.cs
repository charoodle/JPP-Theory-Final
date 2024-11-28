using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [SerializeField] protected GameObject[] enemyPrefabs;
    [SerializeField] protected BoxCollider spawnBox;
    [SerializeField] protected GameObject enemySpawnParticles;

#if UNITY_EDITOR
    [SerializeField] protected bool isDebugSpawner;
    [SerializeField] protected int maxSpawnEnemies = 1;
    [SerializeField] protected int enemiesSpawned = 0;
#endif

    protected void Start()
    {
        InvokeRepeating("SpawnEnemy", 3.0f, 2.0f);
    }

    protected void SpawnEnemy()
    {
        StartCoroutine(SpawnEnemyCoroutine());

#if UNITY_EDITOR
        // Debug spawner only - only spawn a set amount of enemies before stop spawning enemies.
        if (isDebugSpawner)
        {
            enemiesSpawned++;
            if (enemiesSpawned >= maxSpawnEnemies)
            {
                // Stop repeating spawning enemies.
                CancelInvoke("SpawnEnemy");
                return;
            }
        }
#endif
    }

    protected IEnumerator SpawnEnemyCoroutine()
    {
        Vector3 enemySpawnPos = GetRandomPosInBox(spawnBox);

        // Spawn particles
        SpawnParticles(enemySpawnPos);

        // Let particle cloud cover enemy spawning
        yield return new WaitForSeconds(0.35f);

        // Spawn enemy
        GameObject enemyGO = SpawnRandomEnemy(enemySpawnPos, spawnBox.transform.rotation);
        EnemyController enemyCtrl = enemyGO.GetComponent<EnemyController>();
        if(!enemyCtrl)
            yield break;

        // Disable enemy movement on spawn
        enemyCtrl.canMove = false;

        // Wait until enemy hits ground to allow movement
        while(!enemyCtrl.isGrounded)
        {
            yield return new WaitForFixedUpdate();
        }
        enemyCtrl.canMove = true;
    }

    protected void SpawnParticles(Vector3 pos)
    {
        Instantiate(enemySpawnParticles, pos, enemySpawnParticles.transform.rotation);
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
