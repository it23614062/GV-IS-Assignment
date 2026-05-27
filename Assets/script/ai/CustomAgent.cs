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
}