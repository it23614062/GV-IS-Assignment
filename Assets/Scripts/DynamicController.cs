using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AutoCustomPathfinder : MonoBehaviour
{
    [Header("Navigation Settings")]
    public Transform targetDestination;
    public float moveSpeed = 5f;
    public float stoppingDistance = 0.1f;

    [Tooltip("How far the target must move to trigger a path recalculation.")]
    public float destinationUpdateThreshold = 0.5f;

    // --- Graph Data Structures ---
    public class NavNode
    {
        public int id;
        public Vector3 center;
        public Vector3[] vertices;
        public List<NavNode> neighbors = new List<NavNode>();

        public float gCost;
        public float hCost;
        public float FCost => gCost + hCost;
        public NavNode parent;
    }

    private List<NavNode> graphNodes = new List<NavNode>();

    // Path tracking variables
    private Vector3 lastTargetPosition;
    private Coroutine movementCoroutine;

    [ContextMenu("Draw Graph in Scene View")]
    public void GenerateGraphInEditor()
    {
        graphNodes.Clear();
        BuildGraphFromNavMesh();
        Debug.Log("Graph generated for visualization!");
    }

    void Start()
    {
        if (targetDestination == null)
        {
            Debug.LogError("No Target Destination assigned!");
            return;
        }

        BuildGraphFromNavMesh();

        // Initialize tracking and calculate initial path
        lastTargetPosition = targetDestination.position;
        FindAndMoveToTarget();
    }

    void Update()
    {
        if (targetDestination != null)
        {
            // Check if the destination has moved significantly
            if (Vector3.Distance(targetDestination.position, lastTargetPosition) > destinationUpdateThreshold)
            {
                lastTargetPosition = targetDestination.position;
                FindAndMoveToTarget();
            }
        }
    }

    private void BuildGraphFromNavMesh()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        Vector3[] vertices = triangulation.vertices;
        int[] indices = triangulation.indices;

        for (int i = 0; i < indices.Length; i += 3)
        {
            NavNode node = new NavNode();
            node.id = i / 3;
            node.vertices = new Vector3[]
            {
                vertices[indices[i]],
                vertices[indices[i + 1]],
                vertices[indices[i + 2]]
            };

            node.center = (node.vertices[0] + node.vertices[1] + node.vertices[2]) / 3f;
            graphNodes.Add(node);
        }

        for (int i = 0; i < graphNodes.Count; i++)
        {
            for (int j = i + 1; j < graphNodes.Count; j++)
            {
                if (ShareAnEdge(graphNodes[i], graphNodes[j]))
                {
                    graphNodes[i].neighbors.Add(graphNodes[j]);
                    graphNodes[j].neighbors.Add(graphNodes[i]);
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

    private void FindAndMoveToTarget()
    {
        NavNode startNode = GetClosestNode(transform.position);
        NavNode endNode = GetClosestNode(targetDestination.position);

        if (startNode == null || endNode == null) return;

        // 1. Get the raw, zigzag path
        List<Vector3> rawPath = CalculateAStar(startNode, endNode);

        if (rawPath != null && rawPath.Count > 0)
        {
            // 2. SMOOTH THE PATH!
            List<Vector3> finalPath = SmoothPath(rawPath);

            // 3. Move along the clean path
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }

            movementCoroutine = StartCoroutine(FollowPath(finalPath));
        }
    }

    private NavNode GetClosestNode(Vector3 position)
    {
        NavNode closestNode = null;
        float minDistance = float.MaxValue;

        foreach (NavNode node in graphNodes)
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

    private List<Vector3> CalculateAStar(NavNode startNode, NavNode targetNode)
    {
        List<NavNode> openSet = new List<NavNode>();
        HashSet<NavNode> closedSet = new HashSet<NavNode>();

        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            NavNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost ||
                   (openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (NavNode neighbor in currentNode.neighbors)
            {
                if (closedSet.Contains(neighbor)) continue;

                float newMovementCostToNeighbor = currentNode.gCost + Vector3.Distance(currentNode.center, neighbor.center);

                if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCostToNeighbor;
                    neighbor.hCost = Vector3.Distance(neighbor.center, targetNode.center);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    private List<Vector3> RetracePath(NavNode startNode, NavNode endNode)
    {
        List<Vector3> path = new List<Vector3>();
        NavNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode.center);
            currentNode = currentNode.parent;
        }

        path.Add(startNode.center);
        path.Reverse();

        path[0] = transform.position;
        path.Add(targetDestination.position);

        return path;
    }

    private IEnumerator FollowPath(List<Vector3> pathPoints)
    {
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < pathPoints.Count)
        {
            Vector3 targetPosition = pathPoints[currentWaypointIndex];
            targetPosition.y = transform.position.y;

            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            {
                currentWaypointIndex++;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Removes unnecessary zig-zags by drawing a straight line between points
    /// and checking if the NavMesh allows direct movement.
    /// </summary>
    private List<Vector3> SmoothPath(List<Vector3> rawPath)
    {
        if (rawPath == null || rawPath.Count <= 2)
            return rawPath;

        List<Vector3> smoothedPath = new List<Vector3>();
        smoothedPath.Add(rawPath[0]); // Always start with the first point

        int currentIndex = 0;

        while (currentIndex < rawPath.Count - 1)
        {
            int furthestVisibleIndex = currentIndex + 1;

            // Look ahead from the end of the path backwards to find the furthest clear line-of-sight
            for (int i = rawPath.Count - 1; i > currentIndex; i--)
            {
                NavMeshHit hit;

                // NavMesh.Raycast returns TRUE if it hits an edge/obstacle.
                // It returns FALSE if there is a clear, unobstructed path.
                if (!NavMesh.Raycast(rawPath[currentIndex], rawPath[i], out hit, NavMesh.AllAreas))
                {
                    furthestVisibleIndex = i;
                    break; // Found the furthest straight line, stop checking
                }
            }

            smoothedPath.Add(rawPath[furthestVisibleIndex]);
            currentIndex = furthestVisibleIndex;
        }

        return smoothedPath;
    }

    private void OnDrawGizmos()
    {
        if (graphNodes == null || graphNodes.Count == 0) return;

        foreach (NavNode node in graphNodes)
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