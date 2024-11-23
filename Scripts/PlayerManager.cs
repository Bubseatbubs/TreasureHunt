using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{
    public GameObject playerPrefab;
    public static PlayerManager instance;
    private static PlayerController playerController;
    private static Vector3 spawnPosition = Vector3.zero;
    private static Quaternion spawnRotation = Quaternion.identity;
    private static float correctionThreshold = 0.1f;
    private static Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    private static PlayerController clientPlayer;
    Vector2 Input;
    Vector2 LastInput;

    /*
    Singleton Pattern: Make sure there's only one PlayerManager
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
        playerController = playerPrefab.GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        if (IsPlayerInitialized())
        {
            // Tell other clients to update player's position
            // Calls NetworkController to handle sending data to other players

            Input = clientPlayer.GetInput();
            if (Input != LastInput)
            {
                // Only send an input if need be
                if (NetworkController.isHost)
                {
                    UDPHost.instance.SendDataToClients(SendPlayerInput());
                }
                else
                {
                    UDPConnection.instance.SendDataToHost(SendPlayerInput());
                }
            }

            LastInput = clientPlayer.GetInput();

        }
    }

    public static void CreateNewPlayer(int id)
    {
        if (players.ContainsKey(id))
        {
            // Player already exists
            Debug.Log("A player with " + id + " already exists.");
            return;
        }
        else
        {
            // Instantiate a new player
            PlayerController player = Instantiate(playerController, spawnPosition, spawnRotation);
            player.AssignID(id);
            players.Add(id, player);
            Debug.Log($"Create player object with ID of {id}");
            if (!IsPlayerInitialized())
            {
                clientPlayer = player;
                Debug.Log($"Player {id} is now client's player!");
            }
        }
    }

    public string SendPlayerPosition()
    {
        string response = "PlayerManager:UpdatePlayerPosition" + clientPlayer.GetID() + ":" +
        clientPlayer.GetXPosition().ToString() + ":" +
        clientPlayer.GetYPosition().ToString();
        return response;
    }

    public string SendPlayerPositions()
    {
        string response = "PlayerManager:UpdatePlayerPositions:";
        foreach (KeyValuePair<int, PlayerController> p in players)
        {
            response += p.Key + "|" + p.Value.GetXPosition() + "|" + p.Value.GetYPosition() + "/";
        }

        return response;
    }

    public static void UpdatePlayerPosition(int id, float x, float y)
    {
        if (!players.ContainsKey(id))
        {
            Debug.Log("Adding player " + id);
            CreateNewPlayer(id);
        }

        Vector2 currentPosition = players[id].GetPosition();
        Vector2 serverPosition = new Vector2(x, y);

        if (Vector2.Distance(currentPosition, serverPosition) > correctionThreshold)
        {
            players[id].SetPosition(Vector2.Lerp(currentPosition, serverPosition, 0.25f));
        }
        else
        {
            players[id].SetPosition(Vector2.Lerp(currentPosition, serverPosition, 0.05f));
        }
    }

    public static void UpdatePlayerPositions(string message)
    {
        string[] playerData = message.Split('/');
        for (int i = 0; i < playerData.Length - 1; i++)
        {
            Debug.Log(playerData[i]);
            string[] currentPlayerData = playerData[i].Split('|');
            int playerId = int.Parse(currentPlayerData[0]);
            float x = float.Parse(currentPlayerData[1]);
            float y = float.Parse(currentPlayerData[2]);
            UpdatePlayerPosition(playerId, x, y);
        }
    }

    public string SendPlayerInput()
    {
        Vector2 Input = clientPlayer.GetInput();
        string response = "PlayerManager:ReceivePlayerInput:" + clientPlayer.GetID() + ":" +
        Input.x + ":" + Input.y;
        return response;
    }

    public static void ReceivePlayerInput(int id, float x, float y)
    {
        Vector2 Input = new Vector2(x, y).normalized;
        players[id].SetInput(Input);
    }

    public static Boolean IsPlayerInitialized()
    {
        return clientPlayer != null;
    }
}