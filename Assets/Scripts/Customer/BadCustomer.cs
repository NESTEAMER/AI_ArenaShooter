using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BadCustomer : MonoBehaviour
{
    //Movements
    public float moveSpeed = 2f;
    public float wanderMoveSpeed = 2f;
    public float stoppingDistanceToTarget = 0.5f;

    //Behaviour props
    public float detectionRadius = 5f;
    public float wanderRadius = 10f;
    public float wanderTimerMin = 2f;
    public float wanderTimerMax = 5f;
    public float patienceDamage = 15f;

    //HP
    public float maxHealth = 60f;
    private float currentHealth;
    public Image healthBar;

    public LayerMask normalCustomerLayer;

    private Rigidbody2D rb;
    private Transform currentTargetCustomer;
    private Vector2 wanderTargetPoint;
    private float currentWanderTimer;
    private BadCustomerState currentState;

    //Anims
    [HideInInspector]
    public Vector2 moveDir;
    [HideInInspector]
    public float lastHorizontalVec;
    [HideInInspector]
    public float lastVerticalVec;

    private enum BadCustomerState
    {
        Wandering,
        Chasing
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        currentState = BadCustomerState.Wandering;
        SetNewWanderDestination();
    }

    // Update is called once per frame
    void Update()
    {
        FindTargetCustomer();
        healthBar.fillAmount = currentHealth / maxHealth;

        switch (currentState)
        {
            case BadCustomerState.Wandering:
                Wander();
                if (currentTargetCustomer != null)
                {
                    currentState = BadCustomerState.Chasing;
                    Debug.Log($"{name} spotted {currentTargetCustomer.name} and started chasing.");
                }
                break;

            case BadCustomerState.Chasing:
                if (currentTargetCustomer != null)
                {
                    Chase();
                }
                else
                {
                    currentState = BadCustomerState.Wandering;
                    SetNewWanderDestination();
                    moveDir = Vector2.zero;
                    rb.velocity = Vector2.zero;
                    Debug.Log($"{name} lost target, resuming wandering.");
                }
                break;
        }

        if (rb.velocity == Vector2.zero && moveDir != Vector2.zero)
        {
            moveDir = Vector2.zero;
        }
    }

    void FindTargetCustomer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, normalCustomerLayer);
        Transform closestCustomer = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("NormalCustomer"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCustomer = hit.transform;
                }
            }
        }
        currentTargetCustomer = closestCustomer;
    }

    void SetNewWanderDestination()
    {
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(wanderRadius * 0.5f, wanderRadius);
        wanderTargetPoint = (Vector2)transform.position + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDistance;
        currentWanderTimer = Random.Range(wanderTimerMin, wanderTimerMax);
        Debug.Log($"{name} wandering towards {wanderTargetPoint}");
    }

    void Wander()
    {
        currentWanderTimer -= Time.deltaTime;
        if (currentWanderTimer <= 0f || Vector2.Distance(transform.position, wanderTargetPoint) < 0.5f)
        {
            SetNewWanderDestination();
        }

        Vector2 direction = (wanderTargetPoint - (Vector2)transform.position).normalized;
        rb.velocity = direction * wanderMoveSpeed;

        moveDir = direction;
        if (direction.x != 0) lastHorizontalVec = direction.x;
        if (direction.y != 0) lastVerticalVec = direction.y;
    }

    void Chase()
    {
        if (currentTargetCustomer == null)
        {
            moveDir = Vector2.zero;
            rb.velocity = Vector2.zero;
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, currentTargetCustomer.position);
        Vector2 direction = Vector2.zero;

        if (distanceToTarget > stoppingDistanceToTarget)
        {
            direction = (currentTargetCustomer.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        moveDir = direction;
        if (direction.x != 0) lastHorizontalVec = direction.x;
        if (direction.y != 0) lastVerticalVec = direction.y;


        if (distanceToTarget > detectionRadius * 1.2f)
        {
            currentTargetCustomer = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == BadCustomerState.Chasing && collision.gameObject.CompareTag("NormalCustomer"))
        {
            NormalCustomer normalCustomer = collision.gameObject.GetComponent<NormalCustomer>();
            if (normalCustomer != null && !normalCustomer.isLeaving)
            {
                Debug.Log($"{name} collided with NormalCustomer: {normalCustomer.name}. Decreasing patience.");
                normalCustomer.DecreasePatience(patienceDamage);

                SetNewWanderDestination();
                currentState = BadCustomerState.Wandering;
                currentTargetCustomer = null;
            }
        }
        else if (collision.gameObject.CompareTag("Crate"))
        {
            CrateBehaviour crate = collision.gameObject.GetComponent<CrateBehaviour>();
            if (crate != null)
            {
                Debug.Log($"{name} hit by a Crate!");
                TakeDamage(20f);
            }
        }
        else if (collision.gameObject.CompareTag("Cart"))
        {
            CartBehaviour cart = collision.gameObject.GetComponent<CartBehaviour>();
            if (cart != null)
            {
                Debug.Log($"{name} hit by a Cart!");
                TakeDamage(60f);
            }
        }
        // else if (collision.gameObject.CompareTag("Broom") && playerIsUsingBroom)
        // {
        //     TakeDamage(broomDamage);
        // }
        // else if (collision.gameObject.CompareTag("ShoppingCart") && playerIsPushingCartFast)
        // {
        //     TakeDamage(cartDamage);
        // }
        else if (!collision.gameObject.CompareTag("Player") && !collision.gameObject.CompareTag("Crate"))
        {
            if (currentState == BadCustomerState.Wandering)
            {
                SetNewWanderDestination();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"{name} has been defeated!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (currentState == BadCustomerState.Wandering)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, wanderTargetPoint);
            Gizmos.DrawWireSphere(wanderTargetPoint, 0.2f);
        }
        else if (currentState == BadCustomerState.Chasing && currentTargetCustomer != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTargetCustomer.position);
        }
    }
}
