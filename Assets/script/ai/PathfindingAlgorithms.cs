using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class PathfindingAlgorithms
{
    public enum Algorithm { AStar, BFS }

    public static List<Vector3> CalculateAStar(NavNode startNode, NavNode targetNode, Vector3 startPos, Vector3 targetPos)
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
                return RetracePath(startNode, targetNode, startPos, targetPos);
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

                    if (!openSet.Contains())
                        openSet.Add(neighbor);
                }
            }
        }
        return null;
    }

    // NEW: Breadth-First Search implementation
    public static List<Vector3> CalculateBFS(NavNode startNode, NavNode targetNode, Vector3 startPos, Vector3 targetPos)
    {
        Queue<NavNode> queue = new Queue<NavNode>();
        HashSet<NavNode> visited = new HashSet<NavNode>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            NavNode currentNode = queue.Dequeue();

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode, startPos, targetPos);
            }

            foreach (NavNode neighbor in currentNode.neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    neighbor.parent = currentNode;
                    queue.Enqueue(neighbor);
                }
            }
        }
        return null;
    }

    private static List<Vector3> RetracePath(NavNode startNode, NavNode endNode, Vector3 startPos, Vector3 targetPos)
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

        // Stitch the actual world positions to the ends of the node path
        path[0] = startPos;
        path.Add(targetPos);

        return path;
    }

    public static List<Vector3> SmoothPath(List<Vector3> rawPath, int areaMask)
    {
        if (rawPath == null || rawPath.Count <= 2)
            return rawPath;

        List<Vector3> smoothedPath = new List<Vector3>();
        smoothedPath.Add(rawPath[0]);

        int currentIndex = 0;

        while (currentIndex < rawPath.Count - 1)
        {
            int furthestVisibleIndex = currentIndex + 1;

            for (int i = rawPath.Count - 1; i > currentIndex; i--)
            {
                // We replaced NavMesh.AllAreas with our specific areaMask
                if (!NavMesh.Raycast(rawPath[currentIndex], rawPath[i], out _, areaMask))
                {
                    furthestVisibleIndex = i;
                    break;
                }
            }

            smoothedPath.Add(rawPath[furthestVisibleIndex]);
            currentIndex = furthestVisibleIndex;
        }

        return smoothedPath;
    }
}