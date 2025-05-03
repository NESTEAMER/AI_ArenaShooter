using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBlock : MonoBehaviour
{
    //griddy
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public bool displayGridGizmos;

    Node[,] nodeGrid;
    public List<Node> finalPath;

    float nodeDiameter;
    int gridSizeX, gridSizeY;

    public int MaxSize
    {
        get { return gridSizeX * gridSizeY; }
    }

    // Start is called before the first frame update
    void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateGrid()
    {
        nodeGrid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.up * (y * nodeDiameter + nodeRadius);
                bool isWall = Physics2D.OverlapCircle(worldPoint, nodeRadius, unwalkableMask);

                nodeGrid[x, y] = new Node(isWall, worldPoint, x, y);
            }
        }
    }

    public List<Node> GetNeighboringNodes(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(nodeGrid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01((worldPosition.x - (transform.position.x - gridWorldSize.x / 2)) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPosition.y - (transform.position.y - gridWorldSize.y / 2)) / gridWorldSize.y);

        int x = Mathf.Clamp(Mathf.RoundToInt((gridSizeX - 1) * percentX), 0, gridSizeX -1);
        int y = Mathf.Clamp(Mathf.RoundToInt((gridSizeY - 1) * percentY), 0, gridSizeY - 1);

        return nodeGrid[x, y];
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        if (nodeGrid != null && displayGridGizmos)
        {
            foreach (Node n in nodeGrid)
            {
                Gizmos.color = n.IsWall ? Color.red : Color.white;

                if (finalPath != null && finalPath.Contains(n))
                {
                    Gizmos.color = Color.black;
                }
                Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
            }
        }
    }
}
