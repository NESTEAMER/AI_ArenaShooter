using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuddleBehaviour : MonoBehaviour
{
    [Header("Interaction")]
    public float patienceDamageOnStep = 7f;

    [Header("Cleanup")]
    public int hitsToClean = 2;
    private int currentCleanupHits = 0;

    public EventSpawner parentSpawner;

    // Start is called before the first frame update
    void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NormalCustomer"))
        {
            NormalCustomer customer = other.GetComponent<NormalCustomer>();
            if (customer != null && !customer.isLeaving)
            {
                customer.DecreasePatience(patienceDamageOnStep);
                Debug.Log($"{customer.name} stepped in puddle {name}. Patience decreased.");
            }
        }
        // else if (other.CompareTag("Broom") && playerIsUsingBroom) { HitByCleaningTool(); }
    }

    public void HitByCleaningTool(float cleanAmount = 1)
    {
        currentCleanupHits += (int)cleanAmount;
        if (currentCleanupHits >= hitsToClean)
        {
            CleanUp();
        }
    }

    void CleanUp()
    {
        Debug.Log($"Puddle {name} cleaned up.");
        if (parentSpawner != null)
        {
            parentSpawner.EventDefeatedOrCleaned(gameObject);
        }
        Destroy(gameObject);
    }
}
