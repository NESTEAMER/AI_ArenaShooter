using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartSpawner : MonoBehaviour
{
    public GameObject cartPrefabToSpawn;
    private bool hasSpawnedCarts = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnEnable()
    {
        //Only spawn carts once when this spawner becomes active
        if (!hasSpawnedCarts)
        {
            SpawnCarts();
            hasSpawnedCarts = true;
        }
    }
    
    void SpawnCarts()
    {
        Debug.Log("CartSpawner activated, spawning carts at child positions");
        foreach (Transform spawnPoint in transform)
        {
            if (spawnPoint == this.transform) continue;

            bool alreadyHasCart = false;
            foreach(Transform childOfSpawnPoint in spawnPoint) {
                if(childOfSpawnPoint.GetComponent<CartBehaviour>() != null) {
                    alreadyHasCart = true;
                    break;
                }
            }
            if (!alreadyHasCart) {
                GameObject cartInstance = Instantiate(cartPrefabToSpawn, spawnPoint.position, spawnPoint.rotation, spawnPoint);
                Debug.Log($"Cart spawned at {spawnPoint.name}");
            } else {
                Debug.Log($"Cart already present at {spawnPoint.name}, not spawning another.");
            }
        }
    }
}
