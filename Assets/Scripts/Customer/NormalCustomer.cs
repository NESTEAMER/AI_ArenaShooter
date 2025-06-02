using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class NormalCustomer : MonoBehaviour
{
    private GameHandler gameHandlerRef;
    //move stuff
    public float moveSpeed = 3f;
    public float stoppingDistance = 0.2f;

    //necessary logic stuff
    private Rigidbody2D rb;
    private Transform shelvesParent;
    private Transform cashierQueuePointsParent;
    private Transform cashierTarget;
    private Transform exitPoint;
    private List<Transform> allShelves = new List<Transform>();
    private List<Transform> visitedShelves = new List<Transform>();
    private Transform currentTarget;
    private int shelvesToVisitCount;
    private CustomerState currentState;
    public bool isLeaving = false;
    public Image patienceBar;
    public Canvas barCanvas;

    //cust stats
    //public int customerValue;
    public int baseValuePerItem = 20;
    public float maxPatience = 100f;
    public float currentPatience;
    public float patienceDecreaseRate = 1.0f;
    public float patienceLossOnEmptyShelf = 10f;

    //queue stuff
    private List<QueuePoint> availableQueuePoints = new List<QueuePoint>();
    public QueuePoint occupiedPoint = null;
    private float queueCheckCooldown = 0.5f;
    private float timeSinceLastQueueCheck = 0f;
    private int currentQueueIndex = -1;
    public bool isBeingServed { get; private set; } = false;

    //shopping behaviour
    public int itemsToCollectMin = 1;
    public int itemsToCollectMax = 3;
    private int targetItemsToCollect;
    public float shoppingTimeAtShelf = 1.0f;
    private int itemsCollected = 0;
    private float currentShoppingTimer = 0f;
    private ShelfBehaviour currentShelfTargetScript = null;

    //audio
    public AudioClip entryChimeSound;
    public AudioClip angrySound;
    private AudioSource audioSource;

    [HideInInspector]
    public Vector2 moveDir;
    [HideInInspector]
    public float lastHorizontalVec;
    [HideInInspector]
    public float lastVerticalVec;

    private enum CustomerState
    {
        Idle,
        FindingShelf,
        MovingToShelf,
        ShoppingAtShelf,
        FindingCashier,
        MovingToCashier,
        AtCashier,
        LeavingAngrily,
        MovingToExit
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayOneShot(entryChimeSound);
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;

        //Cust stats
        //customerValue = Random.Range(10, 51);
        gameHandlerRef = GameHandler.Instance;
        currentPatience = maxPatience + gameHandlerRef.globalCustomerMaxPatienceBonus;

        //Shelve parent object finder
        GameObject shelvesObject = GameObject.Find("Shelves");
        if (shelvesObject != null)
        {
            shelvesParent = shelvesObject.transform;
            foreach (Transform child in shelvesParent)//Get all children
            {
                allShelves.Add(child);
            }
            Debug.Log($"Found {allShelves.Count} shelves.");
        }
        else
        {
            currentState = CustomerState.LeavingAngrily;
            return;
        }


        //Cashier Queue Points finder
        GameObject queuePointsParentObj = GameObject.Find("CashierQueuePoints");
        if (queuePointsParentObj != null)
        {

            foreach (Transform child in queuePointsParentObj.transform)
            {
                QueuePoint qp = child.GetComponent<QueuePoint>();
                if (qp != null)
                {
                    availableQueuePoints.Add(qp);
                }
            }
        }

        //Find Exit Point
        GameObject exitObject = GameObject.Find("ExitPoint");
        if (exitObject != null)
        {
            exitPoint = exitObject.transform;
        }

        if (allShelves.Count > 0)
        {
            targetItemsToCollect = Random.Range(itemsToCollectMin, itemsToCollectMax + 1);

            shelvesToVisitCount = Mathf.Min(allShelves.Count, targetItemsToCollect + Random.Range(0, 2));

            Debug.Log($"{gameObject.name} wants to collect {targetItemsToCollect} items, will visit up to {shelvesToVisitCount} shelves.");
            currentState = CustomerState.FindingShelf;
        }
        else
        {
            Debug.LogWarning("No shelves found. Customer will go directly to cashier.");
            currentState = CustomerState.LeavingAngrily;
        }
    }

    void FixedUpdate()
    {
        //Movement Logic
        if (currentTarget != null && (currentState == CustomerState.MovingToShelf || currentState == CustomerState.MovingToCashier || currentState == CustomerState.MovingToExit))
        {
            Vector2 direction = ((Vector2)currentTarget.position - rb.position).normalized;
            moveDir = direction;

            rb.velocity = direction * moveSpeed;

            //Check if close enough to the target
            if (Vector2.Distance(rb.position, currentTarget.position) < stoppingDistance)
            {
                ArrivedAtTarget();
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
            moveDir = Vector2.zero;
        }

        if (moveDir.x != 0)
        {
            lastHorizontalVec = moveDir.x;
        }
        if (moveDir.y != 0)
        {
            lastVerticalVec = moveDir.y;
        }
    }

    void Update()
    {
        patienceBar.fillAmount = currentPatience / maxPatience;
        //State Machine Logic
        if (isLeaving)
        {
            if (currentState != CustomerState.MovingToExit && exitPoint != null)
            {
                InitiateDeparture();
            }
            return;
        }

        switch (currentState)
        {
            case CustomerState.FindingShelf:
                FindNextShelf();
                break;

            case CustomerState.ShoppingAtShelf:
                currentShoppingTimer -= Time.deltaTime;
                if (currentShoppingTimer <= 0f)
                {
                    if (currentShelfTargetScript != null)
                    {
                        if (currentShelfTargetScript.TakeItem(1))//Try to take 1 item
                        {
                            itemsCollected++;
                            Debug.Log($"{gameObject.name} took an item from {currentShelfTargetScript.name}");
                        }
                        else
                        {
                            Debug.Log($"{gameObject.name} found shelf {currentShelfTargetScript.name} empty.");
                            DecreasePatience(patienceLossOnEmptyShelf);//Lose patience for empty shelf
                            if (currentPatience <= 0)
                            {
                                InitiateDeparture(); //Leave if patience runs out
                                return;
                            }
                        }
                    }

                    if (itemsCollected >= targetItemsToCollect)
                    {
                        Debug.Log($"{gameObject.name} has collected all desired items ({itemsCollected}). Proceeding to cashier.");
                        currentState = CustomerState.FindingCashier;
                        GoToCashier();
                    }
                    else
                    {
                        currentState = CustomerState.FindingShelf;
                    }
                    currentTarget = null;
                    currentShelfTargetScript = null;
                }
                break;

            case CustomerState.FindingCashier:
                timeSinceLastQueueCheck += Time.deltaTime;
                if (timeSinceLastQueueCheck >= queueCheckCooldown)
                {
                    timeSinceLastQueueCheck = 0f;
                    FindAndClaimQueueSpot();
                }
                break;

            case CustomerState.AtCashier:
                if (isBeingServed) break;

                if (rb.isKinematic && occupiedPoint != null)
                {
                    transform.position = occupiedPoint.transform.position;
                }

                currentPatience -= Time.deltaTime * patienceDecreaseRate;
                if (currentPatience <= 0) { InitiateDeparture(); break; }
                if (occupiedPoint != null && currentQueueIndex > 0)
                {
                    timeSinceLastQueueCheck += Time.deltaTime;
                    if (timeSinceLastQueueCheck >= queueCheckCooldown)
                    {
                        timeSinceLastQueueCheck = 0f;
                        QueuePoint spotAhead = availableQueuePoints[currentQueueIndex - 1];
                        if (!spotAhead.isOccupied)
                        {
                            occupiedPoint.SetVacant();
                            occupiedPoint = spotAhead;
                            occupiedPoint.SetOccupied(this);
                            currentQueueIndex--;
                            currentTarget = occupiedPoint.transform;

                            rb.isKinematic = false;
                            currentState = CustomerState.MovingToCashier;
                        }
                    }
                }
                break;
        }
    }

    void FindNextShelf()
    {
        if (itemsCollected >= targetItemsToCollect ||
        visitedShelves.Count >= shelvesToVisitCount ||
        visitedShelves.Count >= allShelves.Count)//No more shelves at all
        {
            if (itemsCollected > 0)
            {
                currentState = CustomerState.FindingCashier;
                GoToCashier();
            }
            else
            {
                InitiateDeparture();
            }
            return;
        }

        Transform closestShelfTransform = FindClosestUnvisitedShelf();
        if (closestShelfTransform != null)
        {
            currentShelfTargetScript = closestShelfTransform.GetComponent<ShelfBehaviour>();
            if (currentShelfTargetScript == null)
            {
                visitedShelves.Add(closestShelfTransform);
                currentState = CustomerState.FindingShelf;
                return;
            }

            currentTarget = closestShelfTransform;
            currentState = CustomerState.MovingToShelf;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} couldn't find any more unvisited shelves.");
            if (itemsCollected > 0)
            {
                currentState = CustomerState.FindingCashier;
                GoToCashier();
            }
            else
            {
                InitiateDeparture();
            }
        }
    }

    Transform FindClosestUnvisitedShelf()
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector2 currentPosition = rb.position;

        foreach (Transform potentialTarget in allShelves)
        {
            if (!visitedShelves.Contains(potentialTarget)) //Check if already visited
            {
                Vector2 directionToTarget = (Vector2)potentialTarget.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude; //Use squared distance for comparison
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }
        }
        return bestTarget;
    }

    void GoToCashier()
    {
        currentState = CustomerState.FindingCashier;
        timeSinceLastQueueCheck = queueCheckCooldown;
        Debug.Log($"{gameObject.name} is looking for a cashier spot.");
    }

    void ArrivedAtTarget()
    {
        rb.velocity = Vector2.zero; //Stop
        moveDir = Vector2.zero;     //Anim

        if (currentState == CustomerState.MovingToShelf)
        {
            Debug.Log($"{gameObject.name} arrived at shelf: {currentTarget.name}");
            visitedShelves.Add(currentTarget);
            currentState = CustomerState.ShoppingAtShelf;
            currentShoppingTimer = shoppingTimeAtShelf;
        }
        else if (currentState == CustomerState.MovingToCashier)
        {
            Debug.Log($"{gameObject.name} arrived at cashier.");
            currentTarget = null; //Clear target
            currentState = CustomerState.AtCashier;
            rb.isKinematic = true;
            transform.position = occupiedPoint.transform.position;
        }
        if (currentState == CustomerState.MovingToExit)
        {
            Debug.Log($"{gameObject.name} reached the exit point and is destroyed.");
            Destroy(gameObject);
            return;
        }
    }

    public void DecreasePatience(float amount)
    {
        if (isLeaving) return;

        currentPatience -= amount;
        Debug.Log($"{gameObject.name} patience decreased by {amount}. Current patience: {currentPatience}");

        if (occupiedPoint != null)
        {
            occupiedPoint.SetVacant();
            occupiedPoint = null;
        }

        if (currentPatience <= 0 && currentState != CustomerState.LeavingAngrily && currentState != CustomerState.MovingToExit)
        {
            LeaveAngrily();
        }
    }

    void LeaveAngrily()
    {
        InitiateDeparture();
        audioSource.PlayOneShot(angrySound);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Crate"))
        {

        }
    }

    void FindAndClaimQueueSpot()
    {
        for (int i = 0; i < availableQueuePoints.Count; i++)
        {
            QueuePoint potentialSpot = availableQueuePoints[i];
            if (!potentialSpot.isOccupied)
            {
                //Found an available spot
                occupiedPoint = potentialSpot;
                occupiedPoint.SetOccupied(this);//this cust marks

                currentQueueIndex = i;

                cashierTarget = occupiedPoint.transform;
                currentTarget = cashierTarget;
                currentState = CustomerState.MovingToCashier;
                return;
            }
        }
        Debug.Log($"{gameObject.name} is waiting, no free queue spots, sucks.");
    }

    public void BeingServed()
    {
        if (currentState == CustomerState.AtCashier && !isLeaving)
        {
            isBeingServed = true;
            Debug.Log($"{gameObject.name} is now being attended by cashier, better not steal the divider this time.");
        }
    }

    public void ResetServiceState()
    {
        isBeingServed = false;
    }

    public float GetCheckoutValue()
    {
        if (itemsCollected <= 0) //If they collected nothing, they pay nothing
        {
            return 0f;
        }
        float totalItemValue = itemsCollected * baseValuePerItem;
        float patienceMultiplier = Mathf.Max(0.1f, currentPatience / maxPatience);
        float finalValue = totalItemValue * patienceMultiplier;

        return finalValue;
    }

    public void InitiateDeparture()
    {
        if (isLeaving && currentState == CustomerState.MovingToExit) return;

        if (occupiedPoint != null)
        {
            occupiedPoint.SetVacant();
            occupiedPoint = null;
        }
        isLeaving = true;
        isBeingServed = false;
        // currentPatience = 0;

        rb.isKinematic = false;
        if (exitPoint != null)
        {
            currentTarget = exitPoint;
            currentState = CustomerState.MovingToExit;
            barCanvas.enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }
        currentQueueIndex = -1;
    }

    void OnDestroy()
    {
        if (occupiedPoint != null)
        {
            if (occupiedPoint.occupant == this)
            {
                occupiedPoint.SetVacant();
            }
            occupiedPoint = null;
        }
        currentQueueIndex = -1;
    }
}