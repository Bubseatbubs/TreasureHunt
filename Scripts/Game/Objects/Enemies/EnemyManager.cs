using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject enemyPrefab;
    private static Enemy enemyTemplate;
    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    public static EnemyManager instance;
    private static int nextEnemyID = 0;

    /*
    Singleton Pattern: Make sure there's only one EnemyManager
    */
    void Awake()
    {
        if (instance)
        {
            // Remove if instance exists
            Destroy(gameObject);
            return;
        }

        // No instance yet, set this to it
        instance = this;
        enemyTemplate = enemyPrefab.GetComponent<Enemy>();
    }

    public static void CreateNewEnemy(Vector2 spawnPosition)
    {
        Quaternion rotation = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f));
        Enemy enemy = Instantiate(enemyTemplate, spawnPosition, rotation);
        enemy.AssignID(nextEnemyID);
        enemies.Add(nextEnemyID, enemy);
        Debug.Log($"Create enemy object with ID of {nextEnemyID}");
        nextEnemyID++;
    }

    public string SendenemyPositions()
    {
        string response = "EnemyManager:UpdateEnemies:";
        foreach (KeyValuePair<int, Enemy> e in enemies)
        {
            response += e.Key + "|" + e.Value.GetXPosition() + "|" + e.Value.GetYPosition() + "|" + e.Value.isAngry + "/";
        }

        return response;
    }

    public static void UpdateEnemyState(int id, Vector2 pos, bool isAngry)
    {
        if (!enemies.ContainsKey(id))
        {
            Debug.Log("Adding enemy " + id);
            CreateNewEnemy(pos);
        }

        enemies[id].SetPosition(pos);
        enemies[id].isAngry = isAngry;
    }

    public static void UpdateEnemyStates(string message)
    {
        string[] enemyData = message.Split('/');
        for (int i = 0; i < enemyData.Length - 1; i++)
        {
            string[] currentEnemyData = enemyData[i].Split('|');
            int id = int.Parse(currentEnemyData[0]);
            Vector2 pos = new Vector2(float.Parse(currentEnemyData[1]), float.Parse(currentEnemyData[2]));
            bool isAngry = bool.Parse(currentEnemyData[3]);
            UpdateEnemyState(id, pos, isAngry);
        }
    }

    public static void HideEnemy(int enemyID)
    {
        enemies[enemyID].HideEnemy();
    }

    public void CreateEnemies(int numberOfEnemies)
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            // Create enemy
            CreateNewEnemy(MapGenerator.instance.GetRandomSpawnPosition());
        }
    }
}

