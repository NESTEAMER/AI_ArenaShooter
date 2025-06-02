using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    Rigidbody2D rb;

    //broom properties
    public GameObject broomPrefab;
    public float broomSpawnOffset = 0.7f;
    public float broomCooldown = 0.8f;
    public float broomDisplayTime = 0.5f;

    private float currentBroomCooldown = 0f;
    private PlayerStats playerStats;

    private GameHandler gameHandler;

    [HideInInspector]
    public float lastHorizontalVec;
    [HideInInspector]
    public float lastVerticalVec;

    [HideInInspector]
    public Vector2 moveDir;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody2D>();
        gameHandler = GameHandler.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        InputManagement();

        if (currentBroomCooldown > 0)
        {
            currentBroomCooldown -= Time.deltaTime;
        }

        if (playerStats != null && playerStats.hasBroomWeapon && Input.GetButtonDown("Fire2") && currentBroomCooldown <= 0f)
        {
            UseBroom();
        }
    }

    void FixedUpdate()
    {
        Move();
    }

    void InputManagement()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDir = new Vector2(moveX, moveY).normalized;

        //Save last movement value
        if (moveDir.x != 0)
        {
            lastHorizontalVec = moveDir.x;
        }

        if (moveDir.y != 0)
        {
            lastVerticalVec = moveDir.y;
        }
    }
    void Move()
    {
        rb.velocity = new Vector2(moveDir.x * moveSpeed, moveDir.y * moveSpeed);
    }

    void UseBroom()
    {
        Debug.Log("Player uses broom!");
        currentBroomCooldown = broomCooldown;

        Vector2 attackDirection = Vector2.right;

        if (lastHorizontalVec != 0)
        {
            attackDirection = new Vector2(lastHorizontalVec, 0).normalized;
        }
        else if (lastVerticalVec != 0)
        {
            attackDirection = new Vector2(0, lastVerticalVec).normalized;
        }

        Vector3 localSpawnPos = (Vector3)attackDirection * broomSpawnOffset;
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;

        GameObject broomInstance = Instantiate(broomPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        broomInstance.transform.SetParent(this.transform);
        broomInstance.transform.localPosition = localSpawnPos;

        Animator broomAnimator = broomInstance.GetComponent<Animator>();
        SpriteRenderer broomSpriteRenderer = broomInstance.GetComponentInChildren<SpriteRenderer>();
        if (broomAnimator != null)
        {
            if (attackDirection.x > 0)//Facing Right
            {
                if (broomSpriteRenderer != null)
                {
                    broomAnimator.SetTrigger("DoSwingLeft");
                    if (broomSpriteRenderer != null) broomSpriteRenderer.flipX = true;
                }
            }
            else
            {
                if (broomSpriteRenderer != null)
                {
                    // broomSpriteRenderer.flipX = false;
                }
                broomAnimator.SetTrigger("DoSwingRight");
                if (broomSpriteRenderer != null) broomSpriteRenderer.flipX = false;
            }
        }

        BroomBehaviour broomScript = broomInstance.GetComponent<BroomBehaviour>();
        if (broomScript != null)
        {
            broomScript.SetLifetime(broomDisplayTime);
        }
    }
}
