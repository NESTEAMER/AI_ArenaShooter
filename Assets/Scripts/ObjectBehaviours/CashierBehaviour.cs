using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashierBehaviour : MonoBehaviour
{
    private GameHandler gameHandlerInstance;

    [HideInInspector]
    public QueuePoint checkoutSpot;
    public float processingDuration = 0.2f;


    private bool playerInRange = false;
    private NormalCustomer currentCustomerAtCheckout = null;
    private bool isCurrentlyProcessingACustomer = false;

    private bool isProcessing = false;
    private float currentProcessingTimer = 0f;

    //audio
    private AudioSource audioSource;
    public AudioClip chaChingSound;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        gameHandlerInstance = GameHandler.Instance; 

        GameObject cashierQueuePointsParent = GameObject.Find("CashierQueuePoints");

        if (cashierQueuePointsParent != null && cashierQueuePointsParent.transform.childCount > 0)
        {
            checkoutSpot = cashierQueuePointsParent.transform.GetChild(0).GetComponent<QueuePoint>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Who is at checkout spot
        if (checkoutSpot != null && checkoutSpot.isOccupied && checkoutSpot.occupant != null &&
            !checkoutSpot.occupant.isLeaving && !checkoutSpot.occupant.isBeingServed)
        {
            currentCustomerAtCheckout = checkoutSpot.occupant;
        }
        else if (currentCustomerAtCheckout != null && checkoutSpot != null && (!checkoutSpot.isOccupied || checkoutSpot.occupant != currentCustomerAtCheckout))
        {
            currentCustomerAtCheckout = null;
        }

        //E to process
        if (playerInRange && currentCustomerAtCheckout != null && !isCurrentlyProcessingACustomer && Input.GetKeyDown(KeyCode.E))
        {
            ProcessCustomer(currentCustomerAtCheckout);
        }
    }

    void ProcessCustomer(NormalCustomer customerToProcess)
    {
        isCurrentlyProcessingACustomer = true;
        customerToProcess.BeingServed();

        Debug.Log($"Processing customer: {customerToProcess.name}");

        //With delayer
        StartCoroutine(ProcessCustomerWithDelay(customerToProcess));
    }

    System.Collections.IEnumerator ProcessCustomerWithDelay(NormalCustomer customer)
    {
        if (processingDuration > 0)
        {
            yield return new WaitForSeconds(processingDuration);
        }
        else
        {
            yield return null;
        }

        if (customer == null || !customer.isBeingServed)
        {
            Debug.LogWarning($"Customer {customer?.name ?? "Unknown"} was no longer valid or not being served when trying to finalize checkout.");
            isCurrentlyProcessingACustomer = false;
            yield break;
        }

        FinalizeCheckout(customer);
    }

    void FinalizeCheckout(NormalCustomer customer)
    {
        float moneyEarned = customer.GetCheckoutValue();
        gameHandlerInstance.AddMoney((int)moneyEarned + (int)gameHandlerInstance.bonusCashPerCheckoutAmount);
        Debug.Log($"Finished processing {customer.name}. Earned: {(int)moneyEarned}");

        //Make customer leave their spot and the store
        if (customer.occupiedPoint != null)
        {
            customer.occupiedPoint.SetVacant();
        }
        customer.isLeaving = true;
        customer.ResetServiceState();

        //Just use leaveangrily for efficiency
        customer.SendMessage("LeaveAngrily", SendMessageOptions.DontRequireReceiver);

        isCurrentlyProcessingACustomer = false;
        currentCustomerAtCheckout = null;
        audioSource.PlayOneShot(chaChingSound);
    }

    //Enter and Exit stuff
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered cashier range.");
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited cashier range.");
        }
    }
}
