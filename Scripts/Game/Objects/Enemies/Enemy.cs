using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Stack<Vector2> moveCommands = new Stack<Vector2>();
    public bool isAngry = false;
    
    public float speed = 5.0f;

    [SerializeField]
    private Transform enemyTransform;

    public void MoveToNewPositionInMaze()
    {
        int startX = MapGenerator.instance.ConvertXLocationToGrid((int) enemyTransform.position.x);
        int startY = MapGenerator.instance.ConvertYLocationToGrid((int) enemyTransform.position.y);
        moveCommands = Pathfinding.AStarSearch(MapGenerator.instance.GetMazeCell(startX, startY), MapGenerator.instance.GetRandomMazeCell());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float step = speed * Time.deltaTime;
        if (MapGenerator.instance == null) return;

        if (moveCommands.Count <= 0)
        {
            MoveToNewPositionInMaze();
        }

        enemyTransform.position = Vector2.MoveTowards(enemyTransform.position, moveCommands.Peek(), step);
        
        if (Mathf.Approximately(enemyTransform.position.x, moveCommands.Peek().x) && 
        Mathf.Approximately(enemyTransform.position.y, moveCommands.Peek().y)) {
            moveCommands.Pop();
        }
    }
}
