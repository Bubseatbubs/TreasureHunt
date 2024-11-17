using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    public int port = 6077;
    private Queue<String> commands = new Queue<String>();
    protected int ID = 0;
    private Boolean isHost = false;
    protected PlayerManager playerManager;

    /* 
    Begins a lobby as the host client in the P2P connection.
    Host client acts similar to a server.
    Handles keeping all clients synchronized.
    Host's ID is always 0.
    */
    public void StartAsHost()
    {
        // Create host's player object
        playerManager.CreateNewPlayer(ID);

        // TODO: Set up UDP and TCP hosts
    }

    /* 
    Connect to an already existing host as a client.
    Host sends client an ID to use for the session.
    */
    public void ConnectToHost(string hostIP)
    {
        // TODO: Set up UDP and TCP clients
    }

    public int GetID()
    {
        return ID;
    }

    public Boolean IsHost()
    {
        return isHost;
    }

    protected void AddData(string message)
    {

    }

    protected void HandleData(string message)
    {
        // Parse incoming data, send to relevant managers
        string[] data = message.Split(':');
        if (data.Length < 2)
        {
            Debug.Log("Invalid command format.");
            return;
        }

        if (data[1] == "UpdatePlayerPosition")
        {
            int playerId = int.Parse(data[2]);
            float x = float.Parse(data[3]);
            float y = float.Parse(data[4]);

            PlayerManager.instance.UpdatePlayerPosition(playerId, x, y);
        }
        else if (data[1] == "ReceivePlayerInput")
        {
            int playerId = int.Parse(data[2]);
            float x = float.Parse(data[3]);
            float y = float.Parse(data[4]);
            PlayerManager.instance.ReceivePlayerInput(playerId, x, y);
        }
        else if (data[1] == "UpdatePlayerPositions")
        {
            for (int i = 2; i < data.Length; i++)
            {
                string[] playerData = data[i].Split('|');
                int playerId = int.Parse(playerData[0]);
                float x = float.Parse(playerData[1]);
                float y = float.Parse(playerData[2]);
                PlayerManager.instance.UpdatePlayerPosition(playerId, x, y);
            }
        }
    }
}
