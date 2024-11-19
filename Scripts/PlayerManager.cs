using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerManager : MonoBehaviour
{

    public PlayerController player;
    public static PlayerManager instance;
    public Vector3 spawnPosition = Vector3.zero;
    public Quaternion spawnRotation = Quaternion.identity;
    private Dictionary<int, PlayerController> players = new Dictionary<int, PlayerController>();
    PlayerController clientPlayer;
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
    }

    void Update()
    {
        if (IsPlayerInitialized() && !NetworkController.isHost)
        {
            // Tell other clients to update player's position
            // Calls NetworkController to handle sending data to other players
            
            Input = clientPlayer.GetInput();
            if (Input != LastInput)
            {
                // Only send an input if need be
                UDPConnection.instance.SendDataToHost(SendPlayerInput());
            }

            LastInput = clientPlayer.GetInput();
            
        }
    }

    public void CreateNewPlayer(int id)
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
            player = Instantiate(player, spawnPosition, spawnRotation);
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
        string response = "PlayerManager:UpdatePlayerPositions";
        foreach (KeyValuePair<int, PlayerController> p in players)
        {
            response += ":" + p.Key + "|" + p.Value.GetXPosition() + "|" + p.Value.GetYPosition();
        }

        return response;
    }

    public void UpdatePlayerPosition(int id, float x, float y)
    {
        if (!players.ContainsKey(id))
        {
            Debug.Log("Adding player " + id);
            CreateNewPlayer(id);
        }
        players[id].SetPosition(x, y);
    }

    public string SendPlayerInput()
    {
        Vector2 Input = clientPlayer.GetInput();
        string response = "PlayerManager:ReceivePlayerInput:" + clientPlayer.GetID() + ":" +
        Input.x + ":" + Input.y;
        return response;
    }

    public void ReceivePlayerInput(int id, float x, float y)
    {
        Vector2 Input = new Vector2(x, y).normalized;
        players[id].SetInput(Input);
    }

    public Boolean IsPlayerInitialized()
    {
        return clientPlayer != null;
    }
}