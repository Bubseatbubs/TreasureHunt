using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Pathfinding : MonoBehaviour
{
    public Dictionary<MazeCell, MazeCell> cameFrom
        = new Dictionary<MazeCell, MazeCell>();
    public Dictionary<MazeCell, double> costSoFar
        = new Dictionary<MazeCell, double>();

    int interval = 0;

    static public double Heuristic(MazeCell a, MazeCell b)
    {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
    }

    public void AStarSearch(MazeCell start, MazeCell goal)
    {
        var frontier = new PriorityQueue<MazeCell, double>();
        frontier.Enqueue(start, 0);

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();

            if (current.Equals(goal))
            {
                break;
            }

            foreach (var next in current.connections)
            {
                // Add 1 as maze cells are always 1 space away from one another
                double newCost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    double priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        MazeCell backtrackCell = goal;
        int attempts = 0;
        int maxAttempts = 200;
        while (backtrackCell != start || attempts < maxAttempts)
        {
            Debug.DrawLine(backtrackCell.GetWorldPosition(), cameFrom[backtrackCell].GetWorldPosition(), Color.blue, 1f);
            backtrackCell = cameFrom[backtrackCell];
            attempts++;
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("Couldn't generate path!");
        }

        cameFrom.Clear();
        costSoFar.Clear();
    }

    public void FixedUpdate()
    {
        interval++;
        if (interval >= 100)
        {
            interval = 0;
            if (MapGenerator.instance != null)
            {
                AStarSearch(MapGenerator.instance.GetRandomMazeCell(), MapGenerator.instance.GetRandomMazeCell());
            }
        }
    }
}

