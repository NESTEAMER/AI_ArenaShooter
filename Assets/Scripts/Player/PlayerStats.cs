using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Movement")]
    public float baseMoveSpeed = 5f;

    [Header("Current Stats (Modified by PowerUps)")]
    public float currentMoveSpeed;
    public bool hasBroomWeapon = false;
    public bool hasShoppingCartWeapon = false;

    private PlayerMovement playerMovement;
    public float throwForceModifier = 1f;

    public List<string> activePowerUpNames = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        currentMoveSpeed = baseMoveSpeed;
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void ApplyPowerUp(PowerUpEffect powerUp)
    {
        if (activePowerUpNames.Contains(powerUp.powerUpName) && powerUp.isPermanent)
        {
            return;
        }

        Debug.Log($"PlayerStats applying power-up: {powerUp.powerUpName}");
        activePowerUpNames.Add(powerUp.powerUpName);//Track active power-up

        switch (powerUp.effectType)
        {
            case PowerUpType.PlayerSpeedBoost:
                currentMoveSpeed *= powerUp.value;//e.g., 1.2 for 20% boost
                if (playerMovement != null)
                {
                    playerMovement.moveSpeed = currentMoveSpeed;
                }
                Debug.Log($"Player speed updated to: {currentMoveSpeed}");
                break;

            case PowerUpType.WeaponBroom:
                hasBroomWeapon = true;
                Debug.Log("Player now has Broom Weapon!");
                break;

            case PowerUpType.WeaponShoppingCart:
                hasShoppingCartWeapon = true;
                Debug.Log("Player now has Shopping Cart!");
                break;

            case PowerUpType.SlightlyIncreaseThrowForce:
                throwForceModifier *= powerUp.value;
                Debug.Log($"Player throw force updated to: {throwForceModifier}");
                break;

            default:
                Debug.LogWarning($"Power-up type {powerUp.effectType} broken");
                break;
        }
    }

    public void ResetStats()
    {
        currentMoveSpeed = baseMoveSpeed;
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = currentMoveSpeed;
        }
        hasBroomWeapon = false;
        hasShoppingCartWeapon = false;
        activePowerUpNames.Clear();
        Debug.Log("PlayerStats reset to base values.");
    }
}
