using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PestBehaviour : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public float wanderRadius = 5f;
    public float newDestinationInterval = 3f;

    [Header("Interaction")]
    public float patienceDamageOnTouch = 5f;

    [Header("Stats")]
    public float maxHealth = 20f;
    private float currentHealth;

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip pestSpawnSound;

    private Rigidbody2D rb;
    private Vector2 wanderTargetPoint;
    private float currentWanderTimer;
    public Image healthBar;

    public EventSpawner parentSpawner;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        SetNewWanderDestination();

        audioSource.clip = pestSpawnSound;
        audioSource.loop = true;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = currentHealth / maxHealth;
        if (currentHealth <= 0) return;

        currentWanderTimer -= Time.deltaTime;
        if (currentWanderTimer <= 0f || Vector2.Distance(transform.position, wanderTargetPoint) < 0.3f)
        {
            SetNewWanderDestination();
        }

        Vector2 direction = (wanderTargetPoint - (Vector2)transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        if (direction.x < 0) transform.localScale = new Vector3(-1 * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direction.x > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentHealth <= 0) return;

        if (collision.gameObject.CompareTag("NormalCustomer"))
        {
            NormalCustomer customer = collision.gameObject.GetComponent<NormalCustomer>();
            if (customer != null && !customer.isLeaving)
            {
                customer.DecreasePatience(patienceDamageOnTouch);
                Debug.Log($"Pest {name} annoyed {customer.name}.");
                SetNewWanderDestination();
            }
        }
        else if (collision.gameObject.CompareTag("Crate"))
        {
            TakeDamage(10f);
        }
        else if (collision.gameObject.CompareTag("Broom"))
        {
            TakeDamage(20f);
        }
        else if (collision.gameObject.CompareTag("Cart"))
        {
            TakeDamage(20f);
        }
        // else if (collision.gameObject.CompareTag("Broom")) { TakeDamage(20f); }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"Pest {name} eliminated.");
        if (parentSpawner != null)
        {
            parentSpawner.EventDefeatedOrCleaned(gameObject);
        }
        Destroy(gameObject);
    }

    void SetNewWanderDestination()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(wanderRadius * 0.2f, wanderRadius);
        wanderTargetPoint = (Vector2)transform.position + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;
        currentWanderTimer = newDestinationInterval;
    }
}
