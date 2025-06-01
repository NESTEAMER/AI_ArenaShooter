using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyAI_Level2 : MonoBehaviour
{

    [Header("References")]
    public Transform playerTarget;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public TextMeshProUGUI healthText;

    [Header("Stats")]
    public float moveSpeed = 3f;
    public float currentHealth = 100f;
    public float lowHealthThreshold = 25f;
    public float bulletDamage = 25f;

    [Header("Combat Parameters")]
    public float attackRange = 5.0f;
    public float fireRate = 1.0f;
    public float projectileSpeed = 8f;
    private float nextFireTime = 0f;

    [Header("Sight Sensor")]
    public float sightRadius = 10f;
    [Range(0, 360)]
    public float sightAngle = 90f;
    public LayerMask sightTargetMask;
    public LayerMask sightObstacleMask;
    public bool canSeePlayer { get; private set; }
    public Transform playerSeenTargetGO { get; private set; }

    [Header("Investigation Behavior")]
    public float investigationReachThreshold = 0.5f;
    private Vector3 targetInvestigationPosition;
    public float maxInvestigationTime = 5f;
    private float investigationTimer = 0f;

    [Header("Wander Behavior")]
    public float wanderDirectionChangeCooldown = 3f;
    private float currentWanderCooldownTimer = 0f;
    private Vector2 currentWanderDirection = Vector2.zero;


    [Header("Audio Sensor")]
    public float hearingRadius = 7f;
    public LayerMask soundSourceMask;
    public bool heardSound { get; private set; }
    public Vector3 lastHeardSoundPosition { get; private set; }


    //States
    public enum AIState { Patrolling, ChasingPlayer, InvestigatingSound, Attacking, Fleeing }
    public AIState currentState;

    private Rigidbody2D rb;
    public EnemySpawner spawner;
    public PlayerShoot_LV2 ps2;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        healthText.text = currentHealth.ToString();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) playerTarget = playerObject.transform;

        currentState = AIState.Patrolling;
        canSeePlayer = false;
        heardSound = false;
        playerSeenTargetGO = null;

        SetNewWanderDirection();
        currentWanderCooldownTimer = wanderDirectionChangeCooldown;

        StartCoroutine(SensoryRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        RunStateMachine();
    }

    void FixedUpdate()
    {
        ExecuteCurrentStateAction();
    }

    IEnumerator SensoryRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true)
        {
            yield return wait;
            SightSensorCheck();
            AudioSensorCheck();
        }
    }

    void SightSensorCheck()
    {
        canSeePlayer = false;
        playerSeenTargetGO = null;

        if (playerTarget == null) return;//No player, no see

        Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, sightRadius, sightTargetMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            playerSeenTargetGO = target;
            Vector2 directionToTarget = (target.position - transform.position).normalized;

            if (Vector2.Angle(transform.up, directionToTarget) < sightAngle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, target.position);

                if (!Physics2D.Raycast(transform.position, directionToTarget, distanceToTarget, sightObstacleMask))
                {
                    canSeePlayer = true;
                }
                else
                {
                    canSeePlayer = false;
                }
            }
            else
            {
                canSeePlayer = false;
            }
        }
        else
        {
            canSeePlayer = false;
        }
    }

    void AudioSensorCheck()
    {
        heardSound = false;

        Collider2D[] soundChecks = Physics2D.OverlapCircleAll(transform.position, hearingRadius, soundSourceMask);

        if (soundChecks.Length != 0)
        {
            if (!heardSound)
            {
                heardSound = true;
                lastHeardSoundPosition = soundChecks[0].transform.position;
            }
        }
    }

    #region State Machine
    void RunStateMachine()
    {
        if (playerTarget == null)
        {
            currentState = AIState.Patrolling;//No player, just patrol
            return;
        }

        if (currentHealth <= lowHealthThreshold && currentState != AIState.Fleeing)
        {
            currentState = AIState.Fleeing;
            return;
        }
        if (currentState == AIState.Fleeing)
        {
            if (currentHealth > lowHealthThreshold)
            {
                currentState = AIState.Patrolling;
            }
            return;
        }

        switch (currentState)
        {
            case AIState.Patrolling:
                if (canSeePlayer)
                {
                    currentState = AIState.ChasingPlayer;
                }
                else if (heardSound)
                {
                    currentState = AIState.InvestigatingSound;
                    targetInvestigationPosition = lastHeardSoundPosition;
                    heardSound = false;
                    investigationTimer = 0f;
                }
                break;

            case AIState.ChasingPlayer:
                if (!canSeePlayer)
                {
                    currentState = AIState.InvestigatingSound;
                    if (playerTarget != null)
                    {
                        targetInvestigationPosition = playerTarget.position; 
                    }
                    else
                    {
                        currentState = AIState.Patrolling;
                        break;
                    }
                    heardSound = false;
                    investigationTimer = 0f;
                }
                else if (Vector2.Distance(transform.position, playerTarget.position) <= attackRange)
                {
                    currentState = AIState.Attacking;
                }
                break;

            case AIState.InvestigatingSound:
                investigationTimer += Time.deltaTime;
                if (canSeePlayer)
                {
                    currentState = AIState.ChasingPlayer;
                    investigationTimer = 0f;
                }
                else if (Vector2.Distance(transform.position, targetInvestigationPosition) < investigationReachThreshold)
                {
                    currentState = AIState.Patrolling;
                    heardSound = false;
                    investigationTimer = 0f;
                }
                else if (investigationTimer >= maxInvestigationTime)//Gave up
                {
                    currentState = AIState.Patrolling;
                    heardSound = false;
                    investigationTimer = 0f;
                }
                break;

            case AIState.Attacking:
                if (!canSeePlayer || Vector2.Distance(transform.position, playerTarget.position) > attackRange * 1.1f)//Player out of sight or moved too far
                {
                    currentState = AIState.ChasingPlayer;
                }
                break;
        }
    }

    void ExecuteCurrentStateAction()
    {
        if (playerTarget == null && currentState != AIState.Patrolling) {
            currentState = AIState.Patrolling;
        }

        switch (currentState)
        {
            case AIState.Patrolling:
                Patrol();
                break;
            case AIState.ChasingPlayer:
                ChasePlayer();
                break;
            case AIState.InvestigatingSound:
                Investigate();
                break;
            case AIState.Attacking:
                Attack();
                break;
            case AIState.Fleeing:
                Flee();
                break;
        }
    }
    #endregion

    #region State Actions
    void Patrol()
    {
        currentWanderCooldownTimer -= Time.fixedDeltaTime;

        if (currentWanderCooldownTimer <= 0f)
        {
            SetNewWanderDirection();
            currentWanderCooldownTimer = wanderDirectionChangeCooldown;
        }

        rb.velocity = currentWanderDirection * moveSpeed * 0.5f; 

        if (currentWanderDirection != Vector2.zero)
        {
            AimAtTarget(transform.position + (Vector3)currentWanderDirection);
        }
    }
    void SetNewWanderDirection() {
        currentWanderDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    void ChasePlayer()
    {
        if (playerTarget == null) return;
        AimAtTarget(playerTarget.position);
        Vector2 direction = (playerTarget.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
    }

    void Investigate()
    {
        AimAtTarget(targetInvestigationPosition);
        Vector2 direction = (targetInvestigationPosition - transform.position).normalized;
        rb.velocity = direction * moveSpeed * 0.8f;
    }

    void Attack()
    {
        if (playerTarget == null) return;
        rb.velocity = Vector2.zero;
        AimAtTarget(playerTarget.position);
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Flee() {
        if (playerTarget == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }
        Vector2 directionAwayFromPlayer = (transform.position - playerTarget.position).normalized;
        rb.velocity = directionAwayFromPlayer * moveSpeed;
    }
    #endregion

    #region Shoot and health
    void AimAtTarget(Vector3 targetPosition)
    {
        Vector2 lookDir = targetPosition - transform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle; //Quick rotation this time
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
            Destroy(other.gameObject);

            if (currentHealth <= 0)
            {
                spawner = FindObjectOfType<EnemySpawner>();
                spawner.EnemyDestroyed();
                Destroy(gameObject);

                ps2 = FindObjectOfType<PlayerShoot_LV2>();
                ps2.KillCount();
            }
        }
    }
    #endregion
}
