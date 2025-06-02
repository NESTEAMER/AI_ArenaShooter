using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBoundary : MonoBehaviour //extra layer for the already first layered solid boundary wall
{
    //default tp 0,0,0
    public Vector3 exactTeleportPoint = Vector3.zero;

    //defined area
    public bool useRandomTeleportArea = false;
    public Vector2 randomAreaMin = new Vector2(-1f, -1f);
    public Vector2 randomAreaMax = new Vector2(1f, 1f);

    public string[] tagsToTeleport = { "BadCustomer", "Pest", "NormalCustomer", "Player" };

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
        bool shouldTeleport = false;
        foreach (string tag in tagsToTeleport)
        {
            if (other.CompareTag(tag))
            {
                shouldTeleport = true;
                break;
            }
        }

        if (shouldTeleport)
        {
            Rigidbody2D otherRb = other.attachedRigidbody;
            Transform objectToTeleport = (otherRb != null) ? otherRb.transform : other.transform;

            Vector3 teleportToPosition;

            if (useRandomTeleportArea)
            {
                float randomX = Random.Range(randomAreaMin.x, randomAreaMax.x);
                float randomY = Random.Range(randomAreaMin.y, randomAreaMax.y);
                teleportToPosition = new Vector3(randomX, randomY, objectToTeleport.position.z);
            }
            else
            {
                teleportToPosition = new Vector3(exactTeleportPoint.x, exactTeleportPoint.y, objectToTeleport.position.z);
            }
            objectToTeleport.position = teleportToPosition;

            //reset velo just in case
            if (otherRb != null)
            {
                otherRb.velocity = Vector2.zero;
                otherRb.angularVelocity = 0f;
            }
        }
    }
}
