public static List<Vector3> CalculateBFS(NavNode startNode, NavNode targetNode, Vector3 startPos, Vector3 targetPos)
{
    if (startNode == null || targetNode == null) return null;
    if (startNode == targetNode)
        return new List<Vector3> { startPos };

    Queue<NavNode> queue = new Queue<NavNode>();
    HashSet<NavNode> visited = new HashSet<NavNode>();

    queue.Enqueue(startNode);
    visited.Add(startNode);
    startNode.parent = null;

    while (queue.Count > 0)
    {
        NavNode currentNode = queue.Dequeue();

        if (currentNode.neighbors == null) continue;

        foreach (NavNode neighbor in currentNode.neighbors)
        {
            if (neighbor == null || visited.Contains(neighbor)) continue;

            visited.Add(neighbor);
            neighbor.parent = currentNode;
            queue.Enqueue(neighbor);

            if (neighbor == targetNode)
            {
                return RetracePath(startNode, targetNode, startPos, targetPos);
            }
        }
    }

    Debug.LogWarning($"BFS: No path found from {startNode} to {targetNode}");
    return null;
}