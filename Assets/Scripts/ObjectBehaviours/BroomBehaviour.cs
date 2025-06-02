using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroomBehaviour : MonoBehaviour
{
    public float damageToBadEntities = 25f;
    public float damageToBadCustomer = 30f;
    public int cleanupPower = 1;
    private float lifetime;
    private bool lifetimeSet = false;

    //audio
    private AudioSource audioSource;
    public AudioClip sweepSound;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(sweepSound);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Broom hit (Trigger): " + other.gameObject.name);

        if (other.CompareTag("BadCustomer"))
        {
            BadCustomer badCustomer = other.GetComponent<BadCustomer>();
            if (badCustomer != null)
            {
                badCustomer.TakeDamage(damageToBadCustomer);
                Debug.Log("Broom hit BadCustomer: " + other.gameObject.name);
            }
        }
        else if (other.CompareTag("Pest"))
        {
            PestBehaviour pest = other.GetComponent<PestBehaviour>();
            if (pest != null)
            {
                pest.TakeDamage(damageToBadEntities);
                Debug.Log("Broom hit Pest: " + other.gameObject.name);
            }
        }
        else if (other.CompareTag("Puddle"))
        {
            PuddleBehaviour puddle = other.GetComponent<PuddleBehaviour>();
            if (puddle != null)
            {
                puddle.HitByCleaningTool(cleanupPower);
                Debug.Log("Broom hit Puddle: " + other.gameObject.name);
            }
        }
    }
    
    public void SetLifetime(float duration)
    {
        lifetime = duration;
        lifetimeSet = true;
        Destroy(gameObject, lifetime);
    }
}
