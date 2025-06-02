using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    public int heldCrateValue = 0;
    public GameObject heldCrate;
    private bool isHoldingCrate = false;

    [Header("Money & Score")]
    public int currentRunTotalMoney = 0;
    public int currentDayTotalMoney = 0;
    public int currentRoundMoney = 0;
    public TextMeshProUGUI moneyTextUI;
    public Image cashBar;

    [Header("Day & Round Management")]
    public int currentDay = 1;
    public int currentRound = 1;
    public int totalRoundsPerDay = 3;
    public float roundDurationSeconds = 120f;
    public float currentRoundTimer;
    private bool isRoundTransitioning = false;

    [Header("Supply Truck")]
    public GameObject truckPrefab;
    public float truckSpawnIntervalSeconds = 180f;
    private float currentTruckSpawnTimer;
    private bool truckShouldSpawnThisRound = false;

    [Header("UI Elements")]
    public GameObject endOfDayCanvas;
    public GameObject PlayerUICanvas;
    public Button powerUpButton1;
    public TextMeshProUGUI powerUpNameText1;
    public TextMeshProUGUI powerUpDescriptionText1;
    public Button powerUpButton2;
    public TextMeshProUGUI powerUpNameText2;
    public TextMeshProUGUI powerUpDescriptionText2;
    public Button powerUpButton3;
    public TextMeshProUGUI powerUpNameText3;
    public TextMeshProUGUI powerUpDescriptionText3;
    public TextMeshProUGUI quotaStatusText;

    [HideInInspector] public float cashierProcessingTimeModifier = 1f;


    public TextMeshProUGUI timerTextUI;
    public TextMeshProUGUI dayRoundTextUI;

    [Header("Power-ups & Progression")]
    public List<PowerUpEffect> availablePowerUps;
    private List<PowerUpEffect> chosenPowerUpsForDisplay = new List<PowerUpEffect>(3);
    public List<PowerUpEffect> activePlayerPowerUps = new List<PowerUpEffect>();
    private bool isRoundTransitionActive = false;
    private bool isLoadingStageAndWillOfferPowerup = false;
    private bool m_justCompletedStageTransition = false;
    public AudioClip choosePowerUpSound;

    [Header("Round Quota")]
    public int roundQuotaBase = 50;
    public int roundQuotaIncrease = 25;
    private int currentRoundQuota;
    public TextMeshProUGUI roundNumberForQuotaText;

    [Header("Scene Management")]
    public string prologueSceneName = "Prologue";
    public string currentStageLetter = "Prologue";
    private int stageCycleIndex = 0;
    private List<string> stageA_Scenes = new List<string> { "Stage A_1", "Stage A_2", "Stage A_3" };
    private List<string> stageB_Scenes = new List<string> { "Stage B_1", "Stage B_2", "Stage B_3" };
    private List<string> stageC_Scenes = new List<string> { "Stage C_1", "Stage C_2", "Stage C_3" };
    private bool isPrologueCompleted = false;

    [Header("Employee Guidebook UI")]
    public GameObject guidebookPanelParent;
    public List<GameObject> guidebookPages;
    public Button nextPageButton;
    public Button prevPageButton;
    public Button closeGuidebookButton;
    public Button openGuidebookIngameButton;
    public Image arrowAttention;
    public AudioClip openBookSound;

    private int currentPageIndex = 0;
    private bool isGuidebookOpen = false;

    [HideInInspector] public static int finalRunTotalMoney = 0;
    [HideInInspector] public static int finalDayReached = 1;
    [HideInInspector] public static int finalRoundReached = 1;
    [HideInInspector] public static List<string> finalActivePowerUpNames = new List<string>();
    //[HideInInspector] public float globalCustomerPatienceFactor = 1.0f;
    [HideInInspector] public float globalCustomerMaxPatienceBonus = 0f;
    [HideInInspector] public float bonusCashPerCheckoutAmount = 0f;

    private PlayerStats playerStatsInstance;
    private bool isEndOfRoundSequenceActive = false;
    private AudioSource audioSource;

    public static GameHandler Instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        currentRoundTimer = roundDurationSeconds;
        currentTruckSpawnTimer = truckSpawnIntervalSeconds;
        InitializePowerUpButtons();

        FindPlayerStats();
        CalculateCurrentRoundQuota();

        UpdateMoneyDisplay();
        UpdateTimerDisplay();
        UpdateDayRoundDisplay();

        if (endOfDayCanvas != null) endOfDayCanvas.SetActive(false);

        currentDay = 1;
        currentRound = 1;
        currentRunTotalMoney = 0;
        currentDayTotalMoney = 0;
        currentRoundMoney = 0;
        isRoundTransitionActive = false;
        isLoadingStageAndWillOfferPowerup = false;

        if (string.IsNullOrEmpty(currentStageLetter))
        {
            currentStageLetter = "Prologue";
            stageCycleIndex = 0;
            isPrologueCompleted = false;
        }

        StartNewRound();
        Destroy(arrowAttention, 15f);
        if (guidebookPanelParent != null) guidebookPanelParent.SetActive(false);
        if (nextPageButton != null) nextPageButton.onClick.AddListener(ShowNextGuidebookPage);
        if (prevPageButton != null) prevPageButton.onClick.AddListener(ShowPreviousGuidebookPage);
        if (closeGuidebookButton != null) closeGuidebookButton.onClick.AddListener(ToggleGuidebook);
        if (openGuidebookIngameButton != null) openGuidebookIngameButton.onClick.AddListener(ToggleGuidebook);

        ShowGuidebookPageAtIndex(currentPageIndex);
        audioSource = GetComponent<AudioSource>();
    }
    

    // Update is called once per frame
    void Update()
    {
        cashBar.fillAmount = (float)currentRoundMoney / (float)currentRoundQuota;

        if ((float)currentRoundMoney >= (float)currentRoundQuota)
        {
            cashBar.color = Color.green;
            quotaStatusText.color = Color.green;
        }
        else
        {
            cashBar.color = Color.yellow;
            quotaStatusText.color = Color.white;
        }

        if (isEndOfRoundSequenceActive || Time.timeScale == 0f)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.X) && isHoldingCrate)
        {
            DestroyCrate();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleGuidebook();
        }

        if (currentRoundTimer > 0)
        {
            currentRoundTimer -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            EndRound();
        }

        if (!truckShouldSpawnThisRound)
        {
            currentTruckSpawnTimer -= Time.deltaTime;
            if (currentTruckSpawnTimer <= 0f)
            {
                SpawnAndStartTruck();
                currentTruckSpawnTimer = truckSpawnIntervalSeconds;
            }
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //InitializePowerUpButtons();
        }
    }

    public void FindPlayerStats()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerStatsInstance = playerObject.GetComponent<PlayerStats>();
        }
    }

    #region Crate manage
    //Crate management
    public void SetHeldCrate(GameObject crate, int value)
    {
        if (!isHoldingCrate)
        {
            heldCrate = crate;
            heldCrateValue = value;
            isHoldingCrate = true;
            Debug.Log("Held Crate: " + crate.name + ", Value: " + heldCrateValue);
        }
        else
        {
            Debug.LogWarning("Player is already holding a crate!");
        }
    }

    public void ResetHeldCrate()
    {
        heldCrate = null;
        heldCrateValue = 0;
        isHoldingCrate = false;
        Debug.Log("No crate held.");
    }

    public int GetHeldCrateValue()
    {
        return heldCrateValue;
    }

    public void DecreaseHeldCrateValue()
    {
        if (heldCrateValue > 0)
        {
            heldCrateValue--;
            Debug.Log("Crate value decreased to: " + heldCrateValue);
        }
        if (heldCrateValue <= 0 && heldCrate != null)
        {
            Destroy(heldCrate);
            ResetHeldCrate();
            Debug.Log("Held crate destroyed.");
        }
    }

    public bool IsHoldingCrate()
    {
        return isHoldingCrate;
    }

    public void DestroyCrate()
    {
        if (heldCrate != null)
        {
            Destroy(heldCrate);
        }
        ResetHeldCrate();
    }
    #endregion

    #region Money Manage
    //Money management
    public void AddMoney(int amount)
    {
        currentRoundMoney += amount;
        currentDayTotalMoney += amount;
        currentRunTotalMoney += amount;
        UpdateMoneyDisplay();
    }

    void UpdateMoneyDisplay()
    {
        if (moneyTextUI != null)
        {
            if (currentRoundQuota <= 0 && currentRound == 1 && currentDay == 1)
            {
                int initialQuota = roundQuotaBase + (roundQuotaIncrease * (((currentDay - 1) * totalRoundsPerDay) + (currentRound - 1)));
                moneyTextUI.text = $"Cash: {currentRoundMoney:C0} / ${initialQuota:N0}";

            }
            else if (currentRoundQuota <= 0)
            {//if quota isn't set yet for some reason
                moneyTextUI.text = $"Cash: {currentRoundMoney:C0} / Quota: $---";
            }
            else
            {
                moneyTextUI.text = $"Cash: {currentRoundMoney:C0} / ${currentRoundQuota:N0}";
            }
        }
    }
    #endregion

    #region Timer and Day Manage
    //Timer management
    void UpdateTimerDisplay()
    {
        if (timerTextUI != null)
        {
            int minutes = Mathf.FloorToInt(currentRoundTimer / 60F);
            int seconds = Mathf.FloorToInt(currentRoundTimer % 60F);
            timerTextUI.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void UpdateDayRoundDisplay()
    {
        if (dayRoundTextUI != null)
        {
            dayRoundTextUI.text = $"Day: {currentDay} | Round: {currentRound}";
        }
    }

    //Round and day progression
    void EndRound()
    {
        if (isEndOfRoundSequenceActive) return; //Prevent re-entry

        isEndOfRoundSequenceActive = true; //Set flag
        Time.timeScale = 0f; //Pause the game
        Debug.Log($"Round {currentRound} of Day {currentDay} ended. Money this round: {currentRoundMoney}");

        //Quota is for the round just finished
        bool quotaMet = currentRoundMoney >= currentRoundQuota;
        endOfDayCanvas.SetActive(true);


        if (roundNumberForQuotaText != null) roundNumberForQuotaText.text = $"Round {currentRound} Results";

        if (quotaMet)
        {
            Debug.Log("Round quota MET!");
            if (quotaStatusText != null) quotaStatusText.text = $"Quota Met!\nEarned: ${currentRoundMoney} / Quota: ${currentRoundQuota}";

            if (currentRound >= totalRoundsPerDay) //Is it the last round of day/stage?
            {
                //Yes, load next stage THEN offer power-ups.
                if (quotaStatusText != null) quotaStatusText.text += "\nPreparing Next Stage...";
                HidePowerUpSlots(); 
                StartCoroutine(LoadStageThenOfferPowerupsCoroutine());
            }
            else
            {
                //No, Offer power-ups now.
                if (quotaStatusText != null) quotaStatusText.text += "\nChoose a Power-Up:";
                OfferPowerUps();
            }
        }
        else//Quota FAILED
        {
            Debug.Log("Round quota FAILED! Game Over.");
            if (quotaStatusText != null) quotaStatusText.text = $"Quota Failed!\nEarned: ${currentRoundMoney} / Quota: ${currentRoundQuota}\nGAME OVER";
            HidePowerUpSlots();
            LoadGameOverScene();
        }
    }

    void StartNewRound()
    {
        Time.timeScale = 1f;
        isEndOfRoundSequenceActive = false;

        currentRoundTimer = roundDurationSeconds;

        CalculateCurrentRoundQuota();
        UpdateMoneyDisplay();
        UpdateDayRoundDisplay();
        Debug.Log($"Starting Day {currentDay}, Round {currentRound}. Quota: ${currentRoundQuota}");

        truckShouldSpawnThisRound = true;
        SpawnAndStartTruck();
        currentTruckSpawnTimer = truckSpawnIntervalSeconds;
    }

    public void ClockOut()
    {
        currentRoundTimer = 0.1f;
    }

    public void ToggleGuidebook()
    {
        isGuidebookOpen = !isGuidebookOpen;
        audioSource.PlayOneShot(openBookSound);
        
        if (isGuidebookOpen)
        {
            if (guidebookPanelParent != null)
            {
                guidebookPanelParent.SetActive(true);
                Time.timeScale = 0f;//Pause the game
                currentPageIndex = 0;
                ShowGuidebookPageAtIndex(currentPageIndex);
                PlayerUICanvas.SetActive(false);
                Destroy(arrowAttention);
                Debug.Log("Guidebook Opened. Game Paused.");
            }
            else
            {
                isGuidebookOpen = false;
            }
        }
        else
        {
            if (guidebookPanelParent != null) guidebookPanelParent.SetActive(false);
            Time.timeScale = 1f;//Resume the game
            PlayerUICanvas.SetActive(true);
            Debug.Log("Guidebook Closed. Game Resumed.");
        }
    }

    void ShowGuidebookPageAtIndex(int index)
    {
        if (guidebookPages == null || guidebookPages.Count == 0)
        {
            if (nextPageButton != null) nextPageButton.gameObject.SetActive(false);
            if (prevPageButton != null) prevPageButton.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < guidebookPages.Count; i++)
        {
            if (guidebookPages[i] != null)
            {
                guidebookPages[i].SetActive(i == index);
            }
        }

        if (prevPageButton != null) prevPageButton.gameObject.SetActive(index > 0);
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(index < guidebookPages.Count - 1);
    }

    public void ShowNextGuidebookPage()
    {
        if (currentPageIndex < guidebookPages.Count - 1)
        {
            currentPageIndex++;
            ShowGuidebookPageAtIndex(currentPageIndex);
        }
    }

    public void ShowPreviousGuidebookPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowGuidebookPageAtIndex(currentPageIndex);
        }
    }

    #region PowerUps
    void InitializePowerUpButtons()
    {
        if (powerUpButton1 != null) powerUpButton1.onClick.AddListener(() => OnPowerUpChosen(0));
        if (powerUpButton2 != null) powerUpButton2.onClick.AddListener(() => OnPowerUpChosen(1));
        if (powerUpButton3 != null) powerUpButton3.onClick.AddListener(() => OnPowerUpChosen(2));
    }
    void HidePowerUpSlots()
    {
        if (powerUpButton1 != null) powerUpButton1.transform.parent.gameObject.SetActive(false);
        if (powerUpButton2 != null) powerUpButton2.transform.parent.gameObject.SetActive(false);
        if (powerUpButton3 != null) powerUpButton3.transform.parent.gameObject.SetActive(false);
    }

    void OfferPowerUps()
    {
        chosenPowerUpsForDisplay.Clear();
        if (powerUpButton1 != null) powerUpButton1.transform.parent.gameObject.SetActive(true);
        if (powerUpButton2 != null) powerUpButton2.transform.parent.gameObject.SetActive(true);
        if (powerUpButton3 != null) powerUpButton3.transform.parent.gameObject.SetActive(true);

        if (availablePowerUps.Count == 0)
        {
            Debug.LogWarning("No power-ups available to offer.");
            //Hide all power-up slots if no power-ups exist in the master list
            if (powerUpButton1 != null) powerUpButton1.transform.parent.gameObject.SetActive(false);
            if (powerUpButton2 != null) powerUpButton2.transform.parent.gameObject.SetActive(false);
            if (powerUpButton3 != null) powerUpButton3.transform.parent.gameObject.SetActive(false);
        }

        List<PowerUpEffect> tempList = new List<PowerUpEffect>(availablePowerUps);
        int powerUpsToOffer = Mathf.Min(3, tempList.Count);

        for (int i = 0; i < powerUpsToOffer; i++)
        {
            if (tempList.Count == 0) break;
            int randomIndex = Random.Range(0, tempList.Count);
            chosenPowerUpsForDisplay.Add(tempList[randomIndex]);
            tempList.RemoveAt(randomIndex);
        }

        //Display the chosen power-ups or hide slots if fewer than 3 are available
        DisplayPowerUp(chosenPowerUpsForDisplay.Count > 0 ? chosenPowerUpsForDisplay[0] : null, powerUpNameText1, powerUpDescriptionText1, powerUpButton1);
        DisplayPowerUp(chosenPowerUpsForDisplay.Count > 1 ? chosenPowerUpsForDisplay[1] : null, powerUpNameText2, powerUpDescriptionText2, powerUpButton2);
        DisplayPowerUp(chosenPowerUpsForDisplay.Count > 2 ? chosenPowerUpsForDisplay[2] : null, powerUpNameText3, powerUpDescriptionText3, powerUpButton3);
    }

    void DisplayPowerUp(PowerUpEffect powerUp, TextMeshProUGUI nameText, TextMeshProUGUI descText, Button button)
    {
        if (powerUp != null && button != null)
        {
            if (nameText != null) nameText.text = powerUp.powerUpName;
            if (descText != null) descText.text = powerUp.description;
            button.gameObject.SetActive(true);
        }
        else if (button != null)
        {
            button.gameObject.SetActive(false);
        }
    }

    void OnPowerUpChosen(int choiceIndex)
    {
        if (Time.timeScale != 0f)
        {
            Debug.LogWarning("OnPowerUpChosen called while game is not paused!");
            return;
        }

        if (Time.timeScale != 0f && !isEndOfRoundSequenceActive)
        {
            Debug.LogWarning("OnPowerUpChosen called at an unexpected time!");
            return;
        }
        if (choiceIndex < 0 || choiceIndex >= chosenPowerUpsForDisplay.Count || chosenPowerUpsForDisplay[choiceIndex] == null)
        {
            ProceedToNextGameplayState();
            return;
        }

        PowerUpEffect chosenEffect = chosenPowerUpsForDisplay[choiceIndex];
        ApplyPowerUp(chosenEffect);
        Debug.Log("Power-up chosen: " + chosenEffect.powerUpName);

        ProceedToNextGameplayState();
        audioSource.PlayOneShot(choosePowerUpSound);
    }

    void ApplyPowerUp(PowerUpEffect effect)
    {
        if (playerStatsInstance == null) FindPlayerStats();
        if (playerStatsInstance != null)
        {
            switch (effect.effectType)
            {
                case PowerUpType.PlayerSpeedBoost:
                case PowerUpType.WeaponBroom:
                    playerStatsInstance.ApplyPowerUp(effect);
                    break;
                case PowerUpType.WeaponShoppingCart:
                    playerStatsInstance.ApplyPowerUp(effect);

                    GameObject cartSpawnParent = null;
                    List<GameObject> rootObjects = new List<GameObject>();
                    SceneManager.GetActiveScene().GetRootGameObjects(rootObjects);

                    foreach (GameObject obj in rootObjects)
                    {
                        if (obj.name == "CartSpawn")
                        {
                            cartSpawnParent = obj;
                            break;
                        }
                    }

                    if (cartSpawnParent != null)
                    {
                        if (!cartSpawnParent.activeSelf)
                        {
                            cartSpawnParent.SetActive(true);
                            Debug.Log("CartSpawn parent object has been found and enabled in the scene via power-up.");
                        }
                        else
                        {
                            Debug.Log("CartSpawn parent object was already active.");
                        }
                    }
                    break;
            }
        }

        switch (effect.effectType)
        {
            case PowerUpType.CashierSpeedBoost:
                cashierProcessingTimeModifier *= effect.value;
                CashierBehaviour[] cashiers = FindObjectsOfType<CashierBehaviour>();
                foreach (CashierBehaviour cashier in cashiers)
                {
                    cashier.processingDuration *= effect.value;
                }
                Debug.Log($"Cashier processing time modified by factor: {effect.value}");
                break;

            case PowerUpType.IncreaseShelfCapacity:
                ShelfBehaviour[] shelves = FindObjectsOfType<ShelfBehaviour>();
                foreach (ShelfBehaviour shelf in shelves)
                {
                    shelf.IncreaseMaxCapacity((int)effect.value);
                }
                Debug.Log($"Shelf max capacity increased by: {(int)effect.value}");
                break;

            case PowerUpType.IncreaseStartingCrates:
                //numberOfBonusCratesFromPowerUps += (int)effect.value;
                Debug.Log($"Bonus starting crates increased by: {(int)effect.value}");
                break;

            case PowerUpType.AddTimeMax:
                roundDurationSeconds += effect.value;
                //currentRoundTimer += effect.value;
                UpdateTimerDisplay();
                Debug.Log($"Round duration permanently increased by {effect.value} seconds. New base duration: {roundDurationSeconds}");
                break;

            case PowerUpType.CustomerPatienceBoost:
                globalCustomerMaxPatienceBonus += effect.value;
                Debug.Log($"Global customer max patience bonus increased by: {effect.value}");
                break;
                
            case PowerUpType.BonusCashPerCheckout:
                bonusCashPerCheckoutAmount += effect.value;
                Debug.Log($"Cash bonus increased by: {effect.value}");
                break;
        }

        if (effect.isPermanent && !activePlayerPowerUps.Contains(effect))
        {
            activePlayerPowerUps.Add(effect);
        }
    }
    #endregion

    void ProceedToNextGameplayState()
    {
        if (endOfDayCanvas != null) endOfDayCanvas.SetActive(false);
        Time.timeScale = 1f;
        isEndOfRoundSequenceActive = false;

        currentRoundMoney = 0;

        if (m_justCompletedStageTransition)
        {
            m_justCompletedStageTransition = false;
        }
        else
        {
            currentRound++;
        }

        CalculateCurrentRoundQuota();
        UpdateDayRoundDisplay();
        StartNewRound();
    }


    string DetermineNextStageScene()
    {
        string sceneName = "";
        if (currentStageLetter == "Prologue" && !isPrologueCompleted)
        {
            currentStageLetter = "A";
            stageCycleIndex = 0;
            isPrologueCompleted = true;
        }
        else
        {
            stageCycleIndex++;
            if (stageCycleIndex > 2) stageCycleIndex = 0;//Loop A->B->C->A

            if (stageCycleIndex == 0) currentStageLetter = "A";
            else if (stageCycleIndex == 1) currentStageLetter = "B";
            else if (stageCycleIndex == 2) currentStageLetter = "C";
        }

        switch (currentStageLetter)
        {
            case "A": sceneName = stageA_Scenes[Random.Range(0, stageA_Scenes.Count)]; break;
            case "B": sceneName = stageB_Scenes[Random.Range(0, stageB_Scenes.Count)]; break;
            case "C": sceneName = stageC_Scenes[Random.Range(0, stageC_Scenes.Count)]; break;
            default: sceneName = prologueSceneName; currentStageLetter = "Prologue"; isPrologueCompleted = false; stageCycleIndex = 0; break;
        }
        return sceneName;
    }

    IEnumerator LoadStageThenOfferPowerupsCoroutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);

        isLoadingStageAndWillOfferPowerup = true;

        currentDayTotalMoney = 0;
        currentDay++;
        currentRound = 1;
        m_justCompletedStageTransition = true;
        
        string nextScene = DetermineNextStageScene();
        if (!string.IsNullOrEmpty(nextScene))
        {
            Debug.Log($"GameHandler: Coroutine - Loading scene: {nextScene}");
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            isLoadingStageAndWillOfferPowerup = false;
            m_justCompletedStageTransition = false;
            LoadGameOverScene();
        }
        yield break;
    }

    //Re-find important objects like player/other scene objects after scene load
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedCallback;
    }
    void OnSceneLoadedCallback(Scene scene, LoadSceneMode mode)
    {
        if (isLoadingStageAndWillOfferPowerup)
        {
            Time.timeScale = 0f;
            if (endOfDayCanvas != null) endOfDayCanvas.SetActive(true);


            if (quotaStatusText != null) quotaStatusText.text = $"Welcome to Stage {currentStageLetter}!\nChoose your reward:";
            OfferPowerUps();
            isLoadingStageAndWillOfferPowerup = false;
        }
        else if (!isRoundTransitionActive)
        {
            Time.timeScale = 1f;
            CalculateCurrentRoundQuota();
            UpdateDayRoundDisplay();
            StartNewRound();
        }
    }
    #endregion

    void SpawnAndStartTruck()
    {
        Vector3 truckInstantiationPosition = new Vector3(-35, 23, 0);
        GameObject truckInstance = Instantiate(truckPrefab, truckInstantiationPosition, Quaternion.identity);
        TruckBehaviour truckScript = truckInstance.GetComponent<TruckBehaviour>();

        truckScript.StartDeliverySequence(45);
        truckShouldSpawnThisRound = false;
    }

    void CalculateCurrentRoundQuota()
    {
        int totalRoundsPlayedSoFar = ((currentDay - 1) * totalRoundsPerDay) + (currentRound - 1);
        currentRoundQuota = roundQuotaBase + (roundQuotaIncrease * totalRoundsPlayedSoFar);
        Debug.Log($"Day {currentDay}, Round {currentRound} Quota: ${currentRoundQuota}");
    }

    void LoadGameOverScene()
    {
        Time.timeScale = 1f;

        finalRunTotalMoney = currentRunTotalMoney;
        finalDayReached = currentDay;
        finalRoundReached = currentRound;
        finalActivePowerUpNames.Clear();
        PlayerUICanvas.SetActive(false);
        endOfDayCanvas.SetActive(false);
        Destroy(arrowAttention);

        foreach (PowerUpEffect powerUp in activePlayerPowerUps)
        {
            finalActivePowerUpNames.Add(powerUp.powerUpName);
        }

        SceneManager.LoadScene("Game Over");
    }
}