using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZoneNavGraph : MonoBehaviour
{
    [Tooltip("The collider defining the boundaries of this specific zone.")]
    public Collider zoneBoundary;

    public List<NavNode> GraphNodes { get; private set; } = new List<NavNode>();

    [ContextMenu("Draw Graph in Scene View")]
    public void GenerateGraphInEditor()
    {
        BuildGraphFromNavMesh();
        Debug.Log($"Graph generated for {gameObject.name}!");
    }

    private void Awake()
    {
        BuildGraphFromNavMesh();
    }

    public void BuildGraphFromNavMesh()
    {
        if (zoneBoundary == null)
        {
            Debug.LogError("Zone Boundary Collider is missing!", this);
            return;
        }

        GraphNodes.Clear();
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Vector3[] vertices = triangulation.vertices;
        int[] indices = triangulation.indices;

        for (int i = 0; i < indices.Length; i += 3)
        {
            Vector3 center = (vertices[indices[i]] + vertices[indices[i + 1]] + vertices[indices[i + 2]]) / 3f;

            // FILTER: Only add this node if it is inside this zone's trigger box
            if (!zoneBoundary.bounds.Contains(center))
            {
                continue;
            }

            NavNode node = new NavNode
            {
                id = i / 3,
                vertices = new Vector3[]
                {
                    vertices[indices[i]],
                    vertices[indices[i + 1]],
                    vertices[indices[i + 2]]
                }
            };

            node.center = center;
            GraphNodes.Add(node);
        }

        // Link neighbors
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

    private void OnDrawGizmosSelected()
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