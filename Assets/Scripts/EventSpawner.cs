using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSpawner : MonoBehaviour
{
    [Header("Event Prefabs")]
    public GameObject puddlePrefab;

    [Tooltip("Pest can be changed for rats or cockroaches, but only 1 pest type can be spawned in scene")]
    public GameObject pestPrefab;
    public GameObject badCustomerPrefab;

    [Header("Spawn Booleans")]
    public bool canSpawnPuddles = true;
    public bool canSpawnPests = true;
    public bool canSpawnBadCustomers = true;

    [Header("Spawn Settings")]
    public float spawnInterval = 30f;
    public int maxTotalActiveEvents = 5;

    private List<Transform> spawnPoints = new List<Transform>();
    private List<GameObject> activeSpawnedEvents = new List<GameObject>();
    private float currentSpawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in transform)
        {
            spawnPoints.Add(child);
        }
        currentSpawnTimer = Random.Range(spawnInterval * 0.5f, spawnInterval * 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        currentSpawnTimer -= Time.deltaTime;

        if (currentSpawnTimer <= 0f)
        {
            currentSpawnTimer = spawnInterval;

            activeSpawnedEvents.RemoveAll(item => item == null);

            if (activeSpawnedEvents.Count < maxTotalActiveEvents)
            {
                AttemptToSpawnEvent();
            }
        }
    }

    void AttemptToSpawnEvent()
    {
        List<System.Action> possibleSpawns = new List<System.Action>();

        if (canSpawnPuddles && puddlePrefab != null) possibleSpawns.Add(SpawnPuddle);
        if (canSpawnPests && pestPrefab != null) possibleSpawns.Add(SpawnPest);
        if (canSpawnBadCustomers && badCustomerPrefab != null) possibleSpawns.Add(SpawnBadCustomer);

        if (possibleSpawns.Count == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, possibleSpawns.Count);
        possibleSpawns[randomIndex].Invoke();
    }
    Transform GetRandomAvailableSpawnPoint()
    {
        if (spawnPoints.Count == 0) return null;
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }

    void SpawnPuddle()
    {
        Transform spawnPoint = GetRandomAvailableSpawnPoint();
        if (spawnPoint != null)
        {
            GameObject puddleInstance = Instantiate(puddlePrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            PuddleBehaviour pb = puddleInstance.GetComponent<PuddleBehaviour>();
            if (pb != null) pb.parentSpawner = this;
            activeSpawnedEvents.Add(puddleInstance);
            Debug.Log("EventSpawner: Spawned Puddle at " + spawnPoint.name);
        }
    }

    void SpawnPest()
    {
        Transform spawnPoint = GetRandomAvailableSpawnPoint();
        if (spawnPoint != null)
        {
            GameObject pestInstance = Instantiate(pestPrefab, spawnPoint.position, spawnPoint.rotation);
            PestBehaviour pb = pestInstance.GetComponent<PestBehaviour>();
            if (pb != null) pb.parentSpawner = this;
            activeSpawnedEvents.Add(pestInstance);
            Debug.Log("EventSpawner: Spawned Pest at " + spawnPoint.name);
        }
    }

    void SpawnBadCustomer()
    {
        Transform spawnPoint = GetRandomAvailableSpawnPoint();
        if (spawnPoint != null)
        {
            GameObject badCustInstance = Instantiate(badCustomerPrefab, spawnPoint.position, spawnPoint.rotation);
            activeSpawnedEvents.Add(badCustInstance);
            Debug.Log("EventSpawner: Spawned BadCustomer at " + spawnPoint.name);
        }
    }

    public void EventDefeatedOrCleaned(GameObject eventObject)
    {
        if (activeSpawnedEvents.Contains(eventObject))
        {
            activeSpawnedEvents.Remove(eventObject);
        }
    }
}
