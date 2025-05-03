using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 3.0f;
    public int maxEnemies = 10;

    public List<Transform> spawnPoints;
    private int currentEnemyCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        spawnPoints = new List<Transform>();
        foreach (Transform child in transform)
            {
                if (child != transform)
                {
                    spawnPoints.Add(child);
                }
            }
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (currentEnemyCount < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform selectedSpawnPoint = spawnPoints[randomIndex];
        
        GameObject newEnemy = Instantiate(enemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
        currentEnemyCount++;
    }

    public void EnemyDestroyed()
    {
        currentEnemyCount--;
        if (currentEnemyCount < 0) 
        {
            currentEnemyCount = 0;
        }
    }
}
