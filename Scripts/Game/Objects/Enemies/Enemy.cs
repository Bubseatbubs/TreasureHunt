using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Rigidbody2D rb2d;
    Stack<Vector2> moveCommands = new Stack<Vector2>();
    public bool isAngry = false;
    public int ID { get; private set; }

    public float speed = 5.0f;
    public int checkTimerInterval = 10;
    int interval = 0;

    [SerializeField]
    private GameObject enemyObject;

    [SerializeField]
    private Transform enemyTransform;

    [SerializeField]
    private LayerMask targetLayers;

    public void MoveToNewPositionInMaze()
    {
        int startX = MapGenerator.instance.ConvertXLocationToGrid((int)enemyTransform.position.x);
        int startY = MapGenerator.instance.ConvertYLocationToGrid((int)enemyTransform.position.y);
        MazeCell start = MapGenerator.instance.GetMazeCell(startX, startY);
        bool foundValidCell = false;
        MazeCell destination = start;
        while (!foundValidCell)
        {
            destination = MapGenerator.instance.GetRandomMazeCell();
            if (destination.connections.Count > 0)
            {
                foundValidCell = true;
            }
        }

        moveCommands = Pathfinding.AStarSearch(MapGenerator.instance.GetMazeCell(startX, startY), destination);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isAngry)
        {
            RoamingState();
        }
        else
        {
            ChasingState();
        }

        if (NetworkController.isHost)
        {
            interval++;
            if (interval < checkTimerInterval) return;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(enemyTransform.position, 6f, targetLayers);
            if (colliders.Length > 0)
            {
                isAngry = true;
            }
        }
    }

    void RoamingState()
    {
        float step = speed * Time.deltaTime;
        if (MapGenerator.instance == null) return;

        if (moveCommands.Count <= 0)
        {
            MoveToNewPositionInMaze();
        }

        enemyTransform.position = Vector2.MoveTowards(enemyTransform.position, moveCommands.Peek(), step);

        if (Mathf.Approximately(enemyTransform.position.x, moveCommands.Peek().x) &&
        Mathf.Approximately(enemyTransform.position.y, moveCommands.Peek().y))
        {
            moveCommands.Pop();
        }
    }

    void ChasingState()
    {
        float step = speed * 2 * Time.deltaTime;
        Vector2 closestPlayerPosition = GetClosestPlayer(12f).position;
        enemyTransform.position = Vector2.MoveTowards(enemyTransform.position, closestPlayerPosition, step);
    }

    Transform GetClosestPlayer(float radius)
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(enemyTransform.position, radius, targetLayers);
        if (players.Length == 0)
        {
            isAngry = false;
            return enemyTransform;
        }

        Transform closestPlayerTransform = null;
        float minimumDistance = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (Collider2D t in players)
        {
            float distance = Vector2.Distance(t.transform.position, currentPos);
            if (distance < minimumDistance)
            {
                closestPlayerTransform = t.transform;
                minimumDistance = distance;
            }
        }
        return closestPlayerTransform;
    }


    public float GetXPosition()
    {
        return rb2d.position.x;
    }

    public float GetYPosition()
    {
        return rb2d.position.y;
    }

    public void AssignID(int id)
    {
        ID = id;
    }

    public void SetPosition(Vector2 pos)
    {
        rb2d.position = pos;
    }

    public void HideEnemy()
    {
        isAngry = false;
        enemyObject.SetActive(false);
    }
}
