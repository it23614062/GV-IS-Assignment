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