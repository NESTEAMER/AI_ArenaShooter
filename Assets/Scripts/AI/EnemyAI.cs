using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyAI : MonoBehaviour
{
    //Settings stuff
    public Transform playerTarget;
    public float moveSpeed = 3f;
    public float currentHealth = 100f;
    public float lowHealthThreshold = 25f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 8f;
    public EnemySpawner spawner;
    public TextMeshProUGUI healthText;

    //Behaviour param
    public float wanderCooldown = 1.5f;
    private float currentWanderCooldown = 0f;
    public float satisfactionRadius = 1.0f;
    public float attackRange = 5.0f;
    public float fireRate = 1.0f;
    private float nextFireTime = 0f;

    //State stuff
    public enum AIState {Fleeing, Wandering, Attacking, Pathfinding}
    public AIState currentState = AIState.Wandering;
    private AIState previousState = AIState.Wandering;
    private AIState stateBeforePathfinding = AIState.Wandering;

    //A* Pathfinding stuff
    public LayerMask obstacleMask;
    public float waypointReachedDistance = 0.2f;
    public float pathRequestCooldown = 0.5f;
    public float timeSinceLastPathRequest = 0f;

    private Pathfinding pathfinder;
    private List<Vector3> currentPath;
    private int currentPathIndex;

    //Misc
    private Rigidbody2D rb;
    private Vector2 currentWanderDirection = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        pathfinder = FindObjectOfType<Pathfinding>();
        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
        }

        healthText.text = currentHealth.ToString();
        SetNewWanderDirection();
        nextFireTime = Time.time + Random.Range(0f, fireRate);
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastPathRequest += Time.deltaTime;
        if (currentState != AIState.Pathfinding || timeSinceLastPathRequest >= pathRequestCooldown)
        {
            MakeDecision();
        }
    }

    void MakeDecision()
    {
        if (currentState == AIState.Pathfinding && currentHealth > lowHealthThreshold) return;

        float distanceToPlayer = playerTarget != null ? Vector2.Distance(transform.position, playerTarget.position) : float.MaxValue;
        bool isHealthLow = currentHealth <= lowHealthThreshold;

        if (isHealthLow)
        {
            CancelPath();
            currentState = AIState.Fleeing;
        }
        else if (distanceToPlayer <= attackRange)
        {
             if (timeSinceLastPathRequest >= pathRequestCooldown) 
             {
                RequestPath(playerTarget.position, AIState.Attacking);
                timeSinceLastPathRequest = 0f;
             } 
             else if (currentState != AIState.Pathfinding) 
             {
                rb.velocity = Vector2.zero;
             }
        }
        else
        {
            CancelPath();
            currentState = AIState.Wandering;
        }
    }

    void FixedUpdate()
    {
        bool shouldAim = false;
        Vector3 lookTarget = playerTarget.position;

        switch (currentState)
        {
            case AIState.Fleeing:
                Flee();
                break;
            case AIState.Wandering:
                Wander();
                break;
            case AIState.Pathfinding:
                if (currentPath != null && currentPathIndex < currentPath.Count)
                {
                    lookTarget = currentPath[currentPathIndex];
                    shouldAim = true;
                    FollowPath();

                    if (stateBeforePathfinding == AIState.Attacking)
                    {
                        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                        
                        //Shoot if close to player OR close to end of path AND player still in range. That's a lot of demands, who made this? the IRS?
                        bool closeToEndOfPath = (currentPathIndex >= currentPath.Count - 1) && (Vector2.Distance(rb.position, currentPath[currentPath.Count - 1]) < satisfactionRadius * 1.5f);
                        if ((distanceToPlayer < satisfactionRadius * 1.5f || closeToEndOfPath) && Time.time >= nextFireTime)
                        {
                            AimAtTarget(playerTarget.position);
                            Shoot();
                            nextFireTime = Time.time + fireRate;
                        }
                    }
                }
                else
                {
                    CancelPath();
                    currentState = AIState.Wandering;
                }
                break;
        }
        if (shouldAim)
        {
            AimAtTarget(lookTarget);
        }
    }

    void AimAtTarget(Vector3 targetPosition)
    {
        Vector2 lookDir = targetPosition - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.MoveRotation(Mathf.LerpAngle(rb.rotation, angle, Time.fixedDeltaTime * 10f));
    }

    void Shoot()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        projectileRb.velocity = firePoint.up * projectileSpeed;

        Destroy(projectile, 3f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Bullet"))
        {
            currentHealth -= 25;
            healthText.text = currentHealth.ToString();
            Destroy(other);

            if(currentHealth <= 0)
            {
                spawner = FindObjectOfType<EnemySpawner>();
                spawner.EnemyDestroyed();
                Destroy(gameObject);
            }
        }
    }

    void Flee()
    {
        Vector2 direction = rb.position - (Vector2)playerTarget.position;
        float distance = direction.magnitude;

        Vector2 desiredVelocity = direction.normalized * moveSpeed;
        rb.velocity = desiredVelocity;
    }

    void Wander()
    {
        currentWanderCooldown -= Time.fixedDeltaTime;

        if (currentWanderCooldown <= 0f)
        {
            SetNewWanderDirection();
            currentWanderCooldown = wanderCooldown;
        }

        rb.velocity = currentWanderDirection * moveSpeed;
    }

    void SetNewWanderDirection()
    {
        currentWanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void OnDrawGizmos()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = currentPathIndex; i < currentPath.Count; i++)
            {
                Gizmos.DrawCube(currentPath[i], Vector3.one * 0.2f);
                if (i > currentPathIndex)
                {
                    Gizmos.DrawLine(currentPath[i - 1], currentPath[i]);
                }
            }
            if(currentPathIndex < currentPath.Count) 
            {
                 Gizmos.DrawLine(transform.position, currentPath[currentPathIndex]);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        //Attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        //Satisfaction radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, satisfactionRadius);
    }

    //A* section
    void RequestPath(Vector3 targetPosition, AIState desiredEndState)
    {
        stateBeforePathfinding = desiredEndState;
        pathfinder.FindPath(transform.position, targetPosition, OnPathFound);
    }

    public void OnPathFound(List<Vector3> newPath, bool pathSuccessful)
    {
        if (pathSuccessful && newPath != null && newPath.Count > 0)
        {
            currentPath = newPath;
            currentPathIndex = 0;
            currentState = AIState.Pathfinding;
            if (Vector3.Distance(transform.position, currentPath[0]) < waypointReachedDistance * 0.5f)
            {
                currentPathIndex++;
            }
        }
        else
        {
             CancelPath();
             currentState = AIState.Wandering;
        }
    }

    void FollowPath()
    {
        //Path complete or invalid checker
        if (currentPath == null || currentPathIndex >= currentPath.Count)
        {
            CancelPath();
            currentState = previousState;
            return;
        }

        //Move
        Vector3 targetWaypoint = currentPath[currentPathIndex];
        Vector2 direction = (targetWaypoint - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        //Check if waypoint reached
        if (Vector2.Distance(rb.position, targetWaypoint) < waypointReachedDistance)
        {
            currentPathIndex++;
        }
    }

    void CancelPath()
    {
        currentPath = null;
        currentPathIndex = 0;
    }
}
