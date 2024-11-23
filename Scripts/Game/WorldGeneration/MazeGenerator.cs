using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

public class MazeGenerator : MonoBehaviour
{

    [SerializeField]
    private MazeCell _mazeCellPrefab;

    [SerializeField]
    private int _mazeWidth;

    [SerializeField]
    private int _mazeDepth;

    [SerializeField]
    private int _scale;

    [SerializeField]
    private int _centerSize;

    private MazeCell[,] _mazeGrid;
    private static int _seed;

    private int x_displacement = 0;
    private int y_displacement = 0;

    public void Instantiate()
    {
        if (NetworkController.isHost) {
            _seed = Random.Range(1000000, 9999999);
            Debug.Log($"Seed: {_seed}");
            Random.InitState(_seed);
        }
        else {
            _seed = RequestSeed();

            Debug.Log($"Seed: {_seed}");
            Random.InitState(_seed);
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
            }
        }

        GenerateMaze(null, _mazeGrid[0, 0]);
        ClearCenter();
    }

    int RequestSeed() {
        string message = TCPConnection.instance.SendAndReceiveDataFromHost("MazeGenerator:SendSeed");
        return int.Parse(message);
    }

    public static void SendSeed() {
        TCPHost.instance.SendDataToClients($"{_seed}");   
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
                _mazeGrid[x, y].ClearAllWalls();
            }
        }
    }

    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
        {
            return;
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

    private int ConvertXLocationToGrid(int x)
    {
        return (x + x_displacement) / _scale;
    }

    private int ConvertYLocationToGrid(int y)
    {
        return (y + y_displacement) / _scale;
    }
}
