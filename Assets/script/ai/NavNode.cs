using UnityEngine;
using System.Collections.Generic;

public class NavNode
{
    public int id;
    public Vector3 center;
    public Vector3[] vertices;
    public List<NavNode> neighbors = new List<NavNode>();

    // Pathfinding state variables
    public float gCost;
    public float hCost;
    public float FCost => gCost + hCost;
    public NavNode parent;

    // Resets pathfinding data so the node can be used in fresh calculations
    public void ResetNode()
    {
        gCost = 0;
        hCost = 0;
        parent = null;
    }
}