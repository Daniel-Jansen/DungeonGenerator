using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph<T>
{
    public Dictionary<T, List<T>> adjacencyList;

    public Graph()
    {
        adjacencyList = new Dictionary<T, List<T>>();
    }

    public void AddNode(T node)
    {
        if (!adjacencyList.ContainsKey(node))
        {
            adjacencyList[node] = new List<T>();
        }
    }

    public void AddEdge(T fromNode, T toNode)
    {
        if (adjacencyList.ContainsKey(fromNode) && adjacencyList.ContainsKey(toNode))
        {
            adjacencyList[fromNode].Add(toNode);
            adjacencyList[toNode].Add(fromNode);
        }
    }

    public List<T> GetNeighbors(T node)
    {
        if (adjacencyList.ContainsKey(node))
        {
            return adjacencyList[node];
        }
        return new List<T>();
    }

    public void PrintGraph()
    {
        foreach (var kvp in adjacencyList)
        {
            Debug.Log("Node: " + kvp.Key);
            Debug.Log("Neighbors: " + string.Join(", ", kvp.Value));
        }
    }

    public IEnumerator BFS(T startNode, HashSet<T> visited)
    {
        Queue<T> queue = new Queue<T>();
        queue.Enqueue(startNode);
        visited.Add(startNode);
        while (queue.Count > 0)
        {
            T currentNode = queue.Dequeue();

            foreach (T neighbor in GetNeighbors(currentNode))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
            yield return null;
        }

        Debug.Log($"Total nodes: {adjacencyList.Count}");
        Debug.Log($"Visited nodes amount: {visited.Count}");
    }
}
