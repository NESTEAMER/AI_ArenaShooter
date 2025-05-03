using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gridX;
    public int gridY;

    public bool IsWall;
    public Vector3 worldPosition;

    public int gCost;
    public int hCost;
    public Node ParentNode;

    public int fCost
    {
        get { return gCost + hCost; }
    }

    public Node(bool isWall, Vector3 worldPos, int gridX, int gridY)
    {
        this.IsWall = isWall;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;

        gCost = 0;
        hCost = 0;
        ParentNode = null;
    }
}
