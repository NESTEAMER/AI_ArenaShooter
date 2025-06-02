using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShelfBehaviour : MonoBehaviour
{
    public int shelfValue;
    public int maxValue;
    private TextMeshProUGUI stockText;
    private GameHandler gameHandler;
    private bool playerInZone = false;
    public Image stockValue;

    // Start is called before the first frame update
    void Start()
    {
        stockText = GetComponentInChildren<TextMeshProUGUI>();
        stockText.enabled = false;
        UpdateStockText();

        //Find the GameHandler
        gameHandler = GameObject.FindObjectOfType<GameHandler>();
        if (gameHandler == null)
        {
            Debug.LogError("GameHandler not found in the scene!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        stockValue.fillAmount = (float)shelfValue / (float)maxValue;
        if (playerInZone && Input.GetKeyDown(KeyCode.E))
        {
            IncreaseStockFromCrate();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            stockText.enabled = true;
            Debug.Log("Player entered the shelf zone.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            stockText.enabled = false;
            Debug.Log("Player exited the shelf zone.");
        }
    }

    public void IncreaseStockFromCrate()
    {
        if (gameHandler.GetHeldCrateValue() > 0 && shelfValue < maxValue)
        {
            shelfValue++;
            gameHandler.DecreaseHeldCrateValue();
            Debug.Log(gameObject.name + " stock increased to: " + shelfValue);
            UpdateStockText();
        }
        else if (shelfValue >= maxValue)
        {
            Debug.Log(gameObject.name + " is already full.");
        }
        else if (gameHandler.GetHeldCrateValue() <= 0)
        {
            Debug.Log("No crate or crate is empty.");
        }
    }

    void UpdateStockText()
    {
        if (stockText != null)
        {
            stockText.text = "Stock: " + shelfValue + "/" + maxValue;
        }
    }

    public bool TakeItem(int amountToTake = 1)
    {
        if (shelfValue >= amountToTake)
        {
            shelfValue -= amountToTake;
            UpdateStockText();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncreaseMaxCapacity(int amount)
    {
        maxValue += amount;
        UpdateStockText();
        Debug.Log($"{gameObject.name} max capacity increased to {maxValue}");
    }
}