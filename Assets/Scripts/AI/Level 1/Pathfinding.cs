using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    GridBlock gridRef;

    void Awake()
    {
        gridRef = GetComponent<GridBlock>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FindPath(Vector3 startPos, Vector3 targetPos, Action<List<Vector3>, bool> callback)
    {
        List<Vector3> waypoints = new List<Vector3>();
        bool pathSuccess = false;

        Node startNode = gridRef.NodeFromWorldPoint(startPos);
        Node targetNode = gridRef.NodeFromWorldPoint(targetPos);

        if (startNode != null && targetNode != null && !startNode.IsWall && !targetNode.IsWall)
        {
            List<Node> openList = new List<Node>();
            HashSet<Node> closedSet = new HashSet<Node>();
            openList.Add(startNode);

            while (openList.Count > 0)
            {
                Node currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].fCost < currentNode.fCost || (openList[i].fCost == currentNode.fCost && openList[i].hCost < currentNode.hCost))
                    {
                        currentNode = openList[i];
                    }
                }

                openList.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in gridRef.GetNeighboringNodes(currentNode))
                {
                    if (neighbour.IsWall || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetManhattanDistance(currentNode, neighbour);

                    if (newMovementCostToNeighbour < neighbour.gCost || !openList.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetManhattanDistance(neighbour, targetNode);
                        neighbour.ParentNode = currentNode;

                        if (!openList.Contains(neighbour))
                        {
                            openList.Add(neighbour);
                        }
                    }
                }
            } 
        }
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        callback(waypoints, pathSuccess);
    }

    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        List<Node> nodePath = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            nodePath.Add(currentNode);
            currentNode = currentNode.ParentNode;
            if (currentNode == null) break;
        }

        List<Vector3> waypoints = new List<Vector3>();
        for (int i = 0; i < nodePath.Count; i++) 
        {
             waypoints.Add(nodePath[i].worldPosition);
        }
        waypoints.Reverse();

        gridRef.finalPath = nodePath;

        return waypoints;
    }

    int GetManhattanDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        return dstX + dstY;
    }
}
