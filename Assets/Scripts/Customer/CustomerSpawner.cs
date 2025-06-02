using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public GameObject customerPrefab; 

    public int maxCustomers = 5; 
    public float spawnCooldown = 3.0f; 

    [Header("Spawn Points")]
    private List<Transform> spawnPoints = new List<Transform>();
    private float currentSpawnTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        //find children /don't take that out of context
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
        currentSpawnTimer = spawnCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        currentSpawnTimer -= Time.deltaTime;

        //cooldown check
        if (currentSpawnTimer <= 0f)
        {
            currentSpawnTimer = spawnCooldown;

            //can spawn checker
            if (CanSpawn())
            {
                SpawnCustomer();
            }
        }
    }

    bool CanSpawn()
    {
        GameObject[] currentCustomers = GameObject.FindGameObjectsWithTag("NormalCustomer");

        return currentCustomers.Length < maxCustomers;
    }

    void SpawnCustomer()
    {
        //Randomizer
        int randomIndex = Random.Range(0, spawnPoints.Count);
        Transform chosenSpawnPoint = spawnPoints[randomIndex];
        Debug.Log($"Spawning customer at {chosenSpawnPoint.name}");
        Instantiate(customerPrefab, chosenSpawnPoint.position, chosenSpawnPoint.rotation);
    }
}
