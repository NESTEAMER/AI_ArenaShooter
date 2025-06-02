using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueuePoint : MonoBehaviour
{
    public bool isOccupied { get; private set; } = false;
    public NormalCustomer occupant { get; private set; } = null;

    public void SetOccupied(NormalCustomer customer)
    {
        if (!isOccupied)
        {
            isOccupied = true;
            occupant = customer;
        }
    }

    public void SetVacant()
    {
        isOccupied = false;
        occupant = null;
    }

    void Update()
    {
        if (isOccupied && occupant == null)
        {
            SetVacant();
        }
    }
}
