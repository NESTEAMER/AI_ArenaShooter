using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
//do not add new power type in the middle of index or else it'll break the types for already made power ups, add new on new index (very bottom)
{
    PlayerSpeedBoost,
    CashierSpeedBoost, //Unused
    WeaponBroom,
    WeaponShoppingCart,
    IncreaseShelfCapacity,
    IncreaseStartingCrates, //Unused? but used in a different way
    AddTimeMax,
    CustomerPatienceBoost,
    BonusCashPerCheckout,
    SlightlyIncreaseThrowForce
}

[CreateAssetMenu(fileName = "NewPowerUp", menuName = "Supermarket/PowerUp Effect")]
public class PowerUpEffect : ScriptableObject
{
    public string powerUpName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon; //cut content
    public PowerUpType effectType;
    public float value;
    public bool isPermanent = true;
}
