using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

public class MapGenerator : MonoBehaviour
{

    [SerializeField]
    private MazeCell _mazeCellPrefab;

    [SerializeField]
    private MazeRoom _mazeRoomPrefab;

    [SerializeField]
    private int _mazeWidth;

    [SerializeField]
    private int _mazeDepth;

    [SerializeField]
    private int _scale;

    [SerializeField]
    private int _centerSize;

    [SerializeField]
    private LayerMask doNotSpawnItemsOn;

    public static MapGenerator instance;

    private MazeCell[,] _mazeGrid;
    private static int _seed;

    private int x_displacement = 0;
    private int y_displacement = 0;

    public void Instantiate()
    {
        if (instance == null)
        {
            instance = this;
        }

        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];
        x_displacement = (_mazeWidth / 2) * _scale;
        y_displacement = (_mazeDepth / 2) * _scale;

        for (int x = -x_displacement; x < x_displacement; x += _scale)
        {
            for (int y = -y_displacement; y < y_displacement; y += _scale)
            {
                int gridX = ConvertXLocationToGrid(x);
                int gridY = ConvertXLocationToGrid(y);
                _mazeGrid[gridX, gridY] = Instantiate(_mazeCellPrefab, new Vector2(x, y), Quaternion.identity);
                _mazeGrid[gridX, gridY].SetPosition(gridX, gridY);
            }
        }

        // Map generation process
        GenerateMaze(null, _mazeGrid[0, 0]); // Make initial maze
        ClearCenter(); // Clear out the center where players spawn in
        CreateRooms(32, 2); // Spawn in rooms
        CreateItems(100);

        DrawConnections(); // For debug visualization
    }

    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();
        ClearWalls(previousCell, currentCell);

        MazeCell nextCell;

        do
        {
            nextCell = GetNextUnvisitedCell(currentCell);
            if (nextCell != null)
            {
                GenerateMaze(currentCell, nextCell);
            }
        } while (nextCell != null);
    }

    private void CreateRooms(int numberOfRooms, int roomSize)
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            MazeCell[] roomCells = new MazeCell[roomSize * 2];
            int j = 0;

            // Find a random spot to place a room
            int originX = Random.Range(roomSize, _mazeWidth - roomSize);
            int originY = Random.Range(roomSize, _mazeDepth - roomSize);

            // If something was already placed there, go to another spot
            if (_mazeGrid[originX, originY].IsRoomCell || _mazeGrid[originX, originY].IsCenterCell)
            {
                i--;
                continue;
            }

            // Clear space
            Debug.Log($"Creating room at {originX} {originY}");
            for (int x = originX; x < originX + roomSize; x++)
            {
                for (int y = originY; y < originY + roomSize; y++)
                {
                    Debug.Log($"Removed maze cell {x}, {y}");
                    _mazeGrid[x, y].ChangeToRoomCell();
                    roomCells[j++] = _mazeGrid[x, y];
                }
            }

            // Add connections between all the cells of the room
            foreach (MazeCell cell in roomCells)
            {
                for (int k = 0; k < roomCells.Length; k++)
                {
                    cell.AddConnection(roomCells[k]);
                }
            }

            // Create room
            float realX = ConvertXGridToLocation(originX) + 2.5f;
            float realY = ConvertYGridToLocation(originY) + 2.5f;
            Instantiate(_mazeRoomPrefab, new Vector2(realX, realY), Quaternion.identity);
        }
    }

    private void CreateItems(int numberOfItems)
    {
        for (int i = 0; i < numberOfItems; i++)
        {
            // Create item
            CreateItem(GetRandomSpawnPosition());
        }
    }

    private void CreateItem(Vector2 spawnPosition)
    {
        ItemManager.CreateNewItem(spawnPosition);
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 spawnPosition = Vector2.zero;
        bool isSpawnPosValid = false;

        int attemptCount = 0;
        int maxAttempts = 200;

        while (!isSpawnPosValid && attemptCount < maxAttempts)
        {
            spawnPosition.x = Random.Range(-ConvertXGridToLocation(_mazeWidth), ConvertXGridToLocation(_mazeWidth));
            spawnPosition.y = Random.Range(-ConvertYGridToLocation(_mazeDepth), ConvertYGridToLocation(_mazeDepth));
            Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPosition, 0.1f, doNotSpawnItemsOn);
            if (colliders.Length == 0)
            {
                isSpawnPosValid = true;
            }

            attemptCount++;
        }

        if (!isSpawnPosValid)
        {
            Debug.LogWarning("Couldn't find valid spawn position");
        }

        return spawnPosition;
    }

    private void PunchOutWalls(int wallNum)
    {
        for (int i = 0; i < wallNum; i++)
        {
            int x = Random.Range(1, _mazeWidth - 1);
            int y = Random.Range(1, _mazeDepth - 1);
            _mazeGrid[x, y].RemoveRandomWall();
            Debug.Log($"Removed random wall from maze cell {x}, {y}");
        }
    }

    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        var unvisitedCells = GetUnvisitedCells(currentCell);
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = ConvertXLocationToGrid((int)currentCell.transform.position.x);
        int y = ConvertYLocationToGrid((int)currentCell.transform.position.y);

        if (x + 1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x + 1, y];

            if (cellToRight.IsVisited == false)
            {
                yield return cellToRight;
            }
        }

        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, y];

            if (cellToLeft.IsVisited == false)
            {
                yield return cellToLeft;
            }
        }

        if (y + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, y + 1];

            if (cellToFront.IsVisited == false)
            {
                yield return cellToFront;
            }
        }

        if (y - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, y - 1];

            if (cellToBack.IsVisited == false)
            {
                yield return cellToBack;
            }
        }
    }

    private void ClearCenter()
    {
        int centerX = _mazeWidth / 2;
        int centerY = _mazeDepth / 2;

        for (int x = centerX - _centerSize; x < centerX + _centerSize; x++)
        {
            for (int y = centerY - _centerSize; y < centerY + _centerSize; y++)
            {
                _mazeGrid[x, y].ChangeToCenterCell();
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
        }

        // Add connection between both cells
        if (!previousCell.connections.Contains(currentCell))
        {
            previousCell.connections.Add(currentCell);
        }

        if (!currentCell.connections.Contains(previousCell))
        {
            currentCell.connections.Add(previousCell);
        }

        // Cell is left, clear from left to right
        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        // Cell is right, clear from right to left
        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        // Cell is above, clear from front to back
        if (previousCell.transform.position.y < currentCell.transform.position.y)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        // Cell is below, clear from back to front
        if (previousCell.transform.position.y > currentCell.transform.position.y)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }

    private void DrawConnections()
    {
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int y = 0; y < _mazeDepth; y++)
            {
                _mazeGrid[x, y].DrawConnections();
            }
        }
    }

    public int ConvertXLocationToGrid(int x)
    {
        return (x + x_displacement) / _scale;
    }

    public int ConvertYLocationToGrid(int y)
    {
        return (y + y_displacement) / _scale;
    }

    private float ConvertXGridToLocation(int x)
    {
        return x * _scale - x_displacement;
    }

    private float ConvertYGridToLocation(int y)
    {
        return y * _scale - y_displacement;
    }

    public bool InBounds(int x, int y)
    {
        return 0 <= x && x < _mazeWidth
            && 0 <= y && y < _mazeDepth;
    }

    public MazeCell GetRandomMazeCell()
    {
        int x = Random.Range(0, _mazeWidth - 1);
        int y = Random.Range(0, _mazeDepth - 1);
        return _mazeGrid[x, y];
    }

    public MazeCell GetMazeCell(int x, int y)
    {
        return _mazeGrid[x, y];
    }
}
