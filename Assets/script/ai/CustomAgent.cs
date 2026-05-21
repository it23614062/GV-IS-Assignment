using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAgent : MonoBehaviour
{
    [Header("Dependencies")]
    public NavGraph navGraph;
    public Transform targetDestination;

    [Header("Navigation Settings")]
    public PathfindingAlgorithms.Algorithm pathfindingMethod = PathfindingAlgorithms.Algorithm.AStar;
    public float moveSpeed = 5f;
    public float stoppingDistance = 0.1f;
    public float destinationUpdateThreshold = 0.5f;
    public bool applySmoothing = true;

    private Vector3 lastTargetPosition;
    private Coroutine movementCoroutine;

    void Start()
    {
        if (navGraph == null)
            navGraph = FindObjectOfType<NavGraph>();

        if (targetDestination != null)
        {
            lastTargetPosition = targetDestination.position;
            FindAndMoveToTarget();
        }
    }

    void Update()
    {
        if (targetDestination != null)
        {
            if (Vector3.Distance(targetDestination.position, lastTargetPosition) > destinationUpdateThreshold)
            {
                lastTargetPosition = targetDestination.position;
                FindAndMoveToTarget();
            }
        }
    }

    private void FindAndMoveToTarget()
    {
        NavNode startNode = navGraph.GetClosestNode(transform.position);
        NavNode endNode = navGraph.GetClosestNode(targetDestination.position);

        if (startNode == null || endNode == null) return;

        // CRITICAL: Reset graph state before calculating a new path!
        navGraph.ResetAllNodes();

        List<Vector3> rawPath = null;

        // Choose Algorithm
        if (pathfindingMethod == PathfindingAlgorithms.Algorithm.AStar)
        {
            rawPath = PathfindingAlgorithms.CalculateAStar(startNode, endNode, transform.position, targetDestination.position);
        }
        else if (pathfindingMethod == PathfindingAlgorithms.Algorithm.BFS)
        {
            rawPath = PathfindingAlgorithms.CalculateBFS(startNode, endNode, transform.position, targetDestination.position);
        }

        if (rawPath != null && rawPath.Count > 0)
        {
            List<Vector3> finalPath = applySmoothing ? PathfindingAlgorithms.SmoothPath(rawPath) : rawPath;

            if (movementCoroutine != null)
                StopCoroutine(movementCoroutine);

            movementCoroutine = StartCoroutine(FollowPath(finalPath));
        }
    }

    private IEnumerator FollowPath(List<Vector3> pathPoints)
    {
        int currentWaypointIndex = 0;

        while (currentWaypointIndex < pathPoints.Count)
        {
            Vector3 targetPosition = pathPoints[currentWaypointIndex];
            targetPosition.y = transform.position.y; // Keep agent flat

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
}