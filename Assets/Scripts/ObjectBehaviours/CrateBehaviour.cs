using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateBehaviour : MonoBehaviour
{
    //public int crateValue = 2;
    private bool isFollowingPlayer = false;
    private Transform playerTransform;
    //public float followSpeed = 2000f;
    private ShelfBehaviour collidingShelf = null;
    private bool playerInCrateZone = false;
    private GameHandler gameHandler;
    private PlayerStats playerStats;

    //Throw properties
    private Rigidbody2D rb;
    public float throwForce = 10f;
    private bool isThrown = false;

    //Audio
    public AudioClip crashSoundClip;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerStats = GameObject.FindAnyObjectByType<PlayerStats>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        //Find the GameHandler
        gameHandler = GameObject.FindObjectOfType<GameHandler>();
        if (gameHandler == null)
        {
            Debug.LogError("GameHandler not found in the scene");
        }

        //Find rb
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            transform.position = playerTransform.position;

            if (Input.GetMouseButtonDown(0))
            {
                ThrowCrate();
            }
        }

        //Shelf interact
        if (collidingShelf != null && Input.GetKeyDown(KeyCode.E) && isFollowingPlayer)
        {
            collidingShelf.IncreaseStockFromCrate();
        }

        //Destroy if 0
        //if (crateValue <= 0)
        //{
        //    Destroy(gameObject);
        //    Debug.Log(gameObject.name + " destroyed.");
        //}

        if (playerInCrateZone && Input.GetKeyDown(KeyCode.E) && !isFollowingPlayer && !gameHandler.IsHoldingCrate())
        {
            isFollowingPlayer = true;
            Debug.Log("Crate is now following the player.");
            gameHandler.SetHeldCrate(gameObject, 3);
            rb.gravityScale = 0; 
            rb.isKinematic = true;
            rb.velocity = Vector2.zero; 
            isThrown = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCrateZone = true;
            Debug.Log("Player entered crate zone. Press 'E' to pick up.");
        }
        else if (other.CompareTag("Shelf"))
        {
            collidingShelf = other.GetComponent<ShelfBehaviour>();
            Debug.Log("Crate entered shelf zone.");
        }
        else if (isThrown) 
        {
            gameHandler.DecreaseHeldCrateValue();
            Debug.Log("Crate hit " + other.gameObject.name + ". Value decreased.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCrateZone = false;
            Debug.Log("Player exited crate zone.");
            if (isFollowingPlayer)
            {
                gameHandler.ResetHeldCrate();
                isFollowingPlayer = false;
            }
        }
        else if (other.CompareTag("Shelf"))
        {
            collidingShelf = null;
            Debug.Log("Crate exited shelf zone.");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isThrown)
        {
            if (audioSource != null)
            {
                if (audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                if (crashSoundClip != null)
                {
                    audioSource.PlayOneShot(crashSoundClip);
                }
            }
        }
    }

    void ThrowCrate()
    {
        if (isFollowingPlayer)
        {
            isFollowingPlayer = false;
            isThrown = true;
            rb.isKinematic = false;
            rb.gravityScale = 1; 
            rb.drag = 0; 

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 throwDirection = (mousePosition - (Vector2)transform.position).normalized;
            rb.AddForce(throwDirection * (throwForce * playerStats.throwForceModifier), ForceMode2D.Impulse);

            gameHandler.ResetHeldCrate(); 
            Debug.Log("Crate Thrown!");
        }
    }
}