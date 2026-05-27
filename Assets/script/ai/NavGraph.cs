using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavGraph : MonoBehaviour
{
    public List<NavNode> GraphNodes { get; private set; } = new List<NavNode>();

    [Tooltip("Type the exact name of the NavMesh area this graph should build from (e.g., 'InvisibleArea1')")]
    public string targetAreaName = "Walkable";

    [ContextMenu("Draw Graph in Scene View")]
    public void GenerateGraphInEditor()
    {
        BuildGraphFromNavMesh();
        Debug.Log($"Graph generated! Target Area: {targetAreaName} | Total Nodes Found: {GraphNodes.Count}");
    }

    private void Awake()
    {
        BuildGraphFromNavMesh();
    }

    public void BuildGraphFromNavMesh()
    {
        GraphNodes.Clear();
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        Vector3[] vertices = triangulation.vertices;
        int[] indices = triangulation.indices;
        int[] areas = triangulation.areas; // Contains the Area ID for each triangle

        // 1. Get the integer ID for the area string typed in the Inspector
        int targetAreaIndex = NavMesh.GetAreaFromName(targetAreaName);

        if (targetAreaIndex == -1)
        {
            Debug.LogError($"NavMesh Area '{targetAreaName}' not found! Check your spelling in the Navigation Window.");
            return;
        }

        // 2. Loop through triangles
        for (int i = 0; i < indices.Length; i += 3)
        {
            int triangleIndex = i / 3;

            // 3. FILTER: Skip this triangle entirely if it doesn't match our target area
            if (areas[triangleIndex] != targetAreaIndex)
            {
                continue;
            }

            NavNode node = new NavNode
            {
                id = triangleIndex,
                vertices = new Vector3[]
                {
                    vertices[indices[i]],
                    vertices[indices[i + 1]],
                    vertices[indices[i + 2]]
                }
            };

            node.center = (node.vertices[0] + node.vertices[1] + node.vertices[2]) / 3f;
            GraphNodes.Add(node);
        }

        // 4. Connect the neighbors (this logic remains unchanged)
        for (int i = 0; i < GraphNodes.Count; i++)
        {
            for (int j = i + 1; j < GraphNodes.Count; j++)
            {
                if (ShareAnEdge(GraphNodes[i], GraphNodes[j]))
                {
                    GraphNodes[i].neighbors.Add(GraphNodes[j]);
                    GraphNodes[j].neighbors.Add(GraphNodes[i]);
                }
            }
        }
    }

    private bool ShareAnEdge(NavNode a, NavNode b)
    {
        int sharedVertices = 0;
        foreach (Vector3 vA in a.vertices)
        {
            foreach (Vector3 vB in b.vertices)
            {
                if (Vector3.Distance(vA, vB) < 0.01f)
                {
                    sharedVertices++;
                    break;
                }
            }
        }
        return sharedVertices >= 2;
    }

    public NavNode GetClosestNode(Vector3 position)
    {
        NavNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (NavNode node in GraphNodes)
        {
            float dist = Vector3.Distance(position, node.center);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestNode = node;
            }
        }
        return closestNode;
    }

    public void ResetAllNodes()
    {
        foreach (var node in GraphNodes)
        {
            node.ResetNode();
        }
    }

    private void OnDrawGizmos()
    {
        if (GraphNodes == null || GraphNodes.Count == 0) return;

        foreach (NavNode node in GraphNodes)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(node.center, 0.15f);

            Gizmos.color = Color.yellow;
            foreach (NavNode neighbor in node.neighbors)
            {
                Gizmos.DrawLine(node.center, neighbor.center);
            }
        }
    }
}