using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckBehaviour : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float deliveryStopX = 0f;
    public float startX = -15f;
    public float exitX = 15f; 

    public GameObject cratePrefab;
    public int numberOfCratesToDrop = 3;
    public float delayBetweenCrateDrops = 0.3f;
    public float delayAfterDroppingAllCrates = 2f;

    public Transform deliveryZone;

    private List<Transform> crateSpawnPoints = new List<Transform>();
    private Rigidbody2D rb;

    //audio
    private AudioSource audioSource;
    public AudioClip truckSpawnSound;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(truckSpawnSound);
        //rb = GetComponent<Rigidbody2D>();
        foreach (Transform child in deliveryZone)
        {
            crateSpawnPoints.Add(child);
        }

        transform.position = new Vector3(this.startX, transform.position.y, transform.position.z);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator DeliveryLifecycle()
    {
        while (Mathf.Abs(transform.position.x - deliveryStopX) > 0.1f)
        {
            MoveTruckTowards(deliveryStopX);
            yield return null;
        }
        SnapToPositionX(deliveryStopX);
        rb.velocity = Vector2.zero;

        for (int i = 0; i < numberOfCratesToDrop; i++)
        {
            if (crateSpawnPoints.Count > 0)
            {
                Transform spawnPoint = crateSpawnPoints[i % crateSpawnPoints.Count];
                Instantiate(cratePrefab, spawnPoint.position, spawnPoint.rotation);
            }
            yield return new WaitForSeconds(delayBetweenCrateDrops);
        }
        yield return new WaitForSeconds(delayAfterDroppingAllCrates);

        while ((startX < exitX && transform.position.x < exitX) || (startX > exitX && transform.position.x > exitX))
        {
            MoveTruckTowards(exitX);
            yield return null;
        }
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public void StartDeliverySequence(float actualStartX)
    {
        this.startX = actualStartX;
        transform.position = new Vector3(this.startX, transform.position.y, transform.position.z);
        gameObject.SetActive(true);

        crateSpawnPoints.Clear();
        foreach (Transform child in deliveryZone)
        {
            crateSpawnPoints.Add(child);
        }
        StartCoroutine(DeliveryLifecycle());
    }

    void MoveTruckTowards(float targetX)
    {
        float currentX = transform.position.x;
        float direction = Mathf.Sign(targetX - currentX);

        rb.velocity = new Vector2(direction * moveSpeed, 0);
    }

    void SnapToPositionX(float targetX)
    {
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
    }
}
