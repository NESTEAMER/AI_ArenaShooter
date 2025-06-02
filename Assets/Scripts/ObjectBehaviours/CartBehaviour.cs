using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartBehaviour : MonoBehaviour
{
    private bool isFollowingPlayer = false;
    private Transform playerTransform;
    private PlayerStats playerStats;
    //public float followSpeed = 2000f;

    private bool playerInCartZone = false;
    private GameHandler gameHandler;

    //Throw properties
    private Rigidbody2D rb;
    public float throwForce = 18f;
    private bool isThrown = false;

    //Audio
    public AudioClip rollingSoundClip;
    public AudioClip crashSoundClip;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerStats = GameObject.FindAnyObjectByType<PlayerStats>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
        gameHandler = GameHandler.Instance;
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isFollowingPlayer && playerTransform != null)
        {
            transform.position = playerTransform.position;

            if (Input.GetMouseButtonDown(0))
            {
                ThrowCart();
            }
        }

        //Recycling crate element cause... lazy
        if (playerInCartZone && Input.GetKeyDown(KeyCode.E) && !isFollowingPlayer && !gameHandler.IsHoldingCrate())
        {
            isFollowingPlayer = true;
            Debug.Log("Cart is now following the player.");
            gameHandler.SetHeldCrate(gameObject, 0);
            rb.gravityScale = 0;
            rb.isKinematic = true;
            rb.velocity = Vector2.zero;
            isThrown = false;
        }

        //Following and rolling audio
        if (isThrown && rb != null && audioSource != null && rollingSoundClip != null)
        {
            if (rb.velocity.sqrMagnitude > 0.1f && (!audioSource.isPlaying || audioSource.clip != rollingSoundClip))
            {
                audioSource.clip = rollingSoundClip;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (rb.velocity.sqrMagnitude <= 0.1f && audioSource.isPlaying && audioSource.clip == rollingSoundClip)
            {
                audioSource.Stop();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCartZone = true;
            Debug.Log("Player entered cart zone. Press 'E' to pick up.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCartZone = false;
            Debug.Log("Player exited cart zone.");
            if (isFollowingPlayer)
            {
                gameHandler.ResetHeldCrate();
                isFollowingPlayer = false;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isThrown)
        {
            if (audioSource != null)
            {
                if (audioSource.isPlaying && audioSource.clip == rollingSoundClip)
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

    void ThrowCart()
    {
        if (isFollowingPlayer)
        {
            isFollowingPlayer = false;
            isThrown = true;
            rb.isKinematic = false;
            rb.gravityScale = 1;
            rb.drag = 0.2f;

            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 throwDirection = (mousePosition - (Vector2)transform.position).normalized;
            rb.AddForce(throwDirection * (throwForce * playerStats.throwForceModifier), ForceMode2D.Impulse);

            gameHandler.ResetHeldCrate();
            Debug.Log("Cart Thrown!");

            if (audioSource != null && rollingSoundClip != null)
            {
                audioSource.clip = rollingSoundClip;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }
}
