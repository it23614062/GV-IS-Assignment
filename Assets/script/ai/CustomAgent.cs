using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomAgent : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("You MUST manually assign the correct NavGraph for this specific agent's area.")]
    public NavGraph navGraph;
    public Transform targetDestination;

    [Tooltip("Assign a GameObject with a BoxCollider here to define the movement boundary rectangle.")]
    public BoxCollider movementBoundary;

    [Header("Navigation Settings")]
    public PathfindingAlgorithms.Algorithm pathfindingMethod = PathfindingAlgorithms.Algorithm.AStar;
    public float moveSpeed = 5f;
    public float stoppingDistance = 0.1f;
    public float destinationUpdateThreshold = 0.5f;
    public bool applySmoothing = true;

    private Vector3 lastTargetPosition;
    private Coroutine movementCoroutine;
    private List<Vector3> debugPath = new List<Vector3>();

    void Start()
    {
        if (navGraph == null)
        {
            Debug.LogError($"[{gameObject.name}] ERROR: No NavGraph assigned! You must drag the specific NavGraph into the inspector slot.", gameObject);
            return;
        }

        if (targetDestination != null)
        {
            lastTargetPosition = targetDestination.position;
            FindAndMoveToTarget();
        }
    }

    void Update()
    {
        if (targetDestination != null && navGraph != null)
        {
            // We evaluate the clamped target position in case the target itself is moving outside the boundary
            Vector3 currentTargetPos = ClampPositionToBoundary(targetDestination.position);

            if (Vector3.Distance(currentTargetPos, lastTargetPosition) > destinationUpdateThreshold)
            {
                lastTargetPosition = currentTargetPos;
                FindAndMoveToTarget();
            }
        }
    }

    private void FindAndMoveToTarget()
    {
        if (navGraph.GraphNodes.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] The NavGraph '{navGraph.targetAreaName}' has 0 nodes. Ensure the NavMesh is baked and area names match.");
            return;
        }

        // Clamp our starting and ending positions to ensure pathing stays within the rectangle
        Vector3 clampedStartPos = ClampPositionToBoundary(transform.position);
        Vector3 clampedEndPos = ClampPositionToBoundary(targetDestination.position);

        NavNode startNode = navGraph.GetClosestNode(clampedStartPos);
        NavNode endNode = navGraph.GetClosestNode(clampedEndPos);

        if (startNode == null || endNode == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Could not find a valid Start or End node on the graph.");
            return;
        }

        // CRITICAL: Reset graph state before calculating a new path!
        navGraph.ResetAllNodes();

        List<Vector3> rawPath = null;

        // Choose Algorithm
        if (pathfindingMethod == PathfindingAlgorithms.Algorithm.AStar)
        {
            rawPath = PathfindingAlgorithms.CalculateAStar(startNode, endNode, clampedStartPos, clampedEndPos);
        }
        else if (pathfindingMethod == PathfindingAlgorithms.Algorithm.BFS)
        {
            rawPath = PathfindingAlgorithms.CalculateBFS(startNode, endNode, clampedStartPos, clampedEndPos);
        }

        if (rawPath != null && rawPath.Count > 0)
        {
            int areaIndex = NavMesh.GetAreaFromName(navGraph.targetAreaName);
            int areaMask = 1 << areaIndex;

            List<Vector3> finalPath = applySmoothing ? PathfindingAlgorithms.SmoothPath(rawPath, areaMask) : rawPath;

            debugPath = finalPath;

            if (movementCoroutine != null)
                StopCoroutine(movementCoroutine);

            movementCoroutine = StartCoroutine(FollowPath(finalPath));
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Pathfinding failed. The start and end nodes are likely disconnected on the {navGraph.targetAreaName} layer.");
        }
    }

    private IEnumerator FollowPath(List<Vector3> pathPoints)
    {
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < pathPoints.Count)
        {
            // Clamp the waypoint to ensure the agent doesn't try to walk out of bounds
            Vector3 targetPosition = ClampPositionToBoundary(pathPoints[currentWaypointIndex]);
            targetPosition.y = transform.position.y; // Keep agent flat

            Vector3 direction = (targetPosition - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
            }

            // Move the agent
            Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Hard clamp the actual transform position to prevent the agent from ever stepping out
            transform.position = ClampPositionToBoundary(newPosition);

            if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            {
                currentWaypointIndex++;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Forces a given position to remain inside the assigned boundary area rectangle.
    /// </summary>
    private Vector3 ClampPositionToBoundary(Vector3 position)
    {
        if (movementBoundary == null)
            return position; // No boundary assigned, allow normal movement

        // Get the closest point on the bounding box to the desired position
        Vector3 clampedPosition = movementBoundary.ClosestPoint(position);

        // Preserve the original Y value so the agent doesn't sink into or float above the ground
        clampedPosition.y = position.y;

        return clampedPosition;
    }

    private void OnDrawGizmos()
    {
        // Draw the path
        if (debugPath != null && debugPath.Count > 0)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < debugPath.Count - 1; i++)
            {
                Gizmos.DrawLine(debugPath[i], debugPath[i + 1]);
                Gizmos.DrawSphere(debugPath[i], 0.1f);
            }
            Gizmos.DrawSphere(debugPath[debugPath.Count - 1], 0.1f);
        }

        // Draw the movement boundary
        if (movementBoundary != null)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Transparent green
            Gizmos.matrix = movementBoundary.transform.localToWorldMatrix;
            Gizmos.DrawCube(movementBoundary.center, movementBoundary.size);
            Gizmos.DrawWireCube(movementBoundary.center, movementBoundary.size);
        }
    }
}