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
        Debug.LogWarning($"[{gameObject.name}] The NavGraph '{navGraph.targetAreaName}' has 0 nodes.");
        return;
    }

    Vector3 clampedStartPos = ClampPositionToBoundary(transform.position);
    Vector3 clampedEndPos = ClampPositionToBoundary(targetDestination.position);

    NavNode startNode = navGraph.GetClosestNode(clampedStartPos);
    NavNode endNode = navGraph.GetClosestNode(clampedEndPos);

    if (startNode == null || endNode == null)
    {
        Debug.LogWarning($"[{gameObject.name}] Could not find a valid Start or End node.");
        return;
    }

    navGraph.ResetAllNodes();

    List<Vector3> rawPath = null;

    if (pathfindingMethod == PathfindingAlgorithms.Algorithm.AStar)
    {
        rawPath = PathfindingAlgorithms.CalculateAStar(startNode, endNode, clampedStartPos, clampedEndPos);
    }
    else if (pathfindingMethod == PathfindingAlgorithms.Algorithm.BFS)
    {
        rawPath = PathfindingAlgorithms.CalculateBFS(startNode, endNode, clampedStartPos, clampedEndPos);
    }
}
}