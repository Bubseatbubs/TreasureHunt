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
    private static Queue<String> commands = new Queue<String>();
    protected static int ID = 0;
    public static Boolean isHost = false;
    protected PlayerManager playerManager;
    private static NetworkController instance;
    TCPHost tcpHost;
    UDPHost udpHost;
    TCPConnection tcpClient;
    UDPConnection udpClient;

    public static NetworkController Instance()
    {
        return instance;
    }

    // Singleton
    void Awake()
    {
        if (instance)
        {
            // Remove if instance exists
            return;
        }

        // No instance yet, set this to it
        instance = this;
        playerManager = FindObjectOfType<PlayerManager>();
    }

    /* 
    Begins a lobby as the host client in the P2P connection.
    Host client acts similar to a Host.
    Handles keeping all clients synchronized.
    Host's ID is always 0.
    */
    public void HostGame(int port)
    {
        if (isHost)
        {
            Debug.Log("Can't host while hosting!");
            return;
        }
        tcpHost = gameObject.AddComponent<TCPHost>();
        udpHost = gameObject.AddComponent<UDPHost>();

        // Prepare TCP and UDP host
        tcpHost.Instantiate(port);
        udpHost.Instantiate(port);

        isHost = true;

        // Create host's player object
        playerManager.CreateNewPlayer(ID);

        Debug.Log("Hosted game!");
    }

    /* 
    Connect to an already existing host as a client.
    Host sends client an ID to use for the session.
    */
    public void ConnectToGame(string hostIP, int port)
    {
        Debug.Log($"Using IP Address: {hostIP} and port {port}");
        if (isHost)
        {
            Debug.Log("Can't connect while hosting!");
            return;
        }
        tcpClient = gameObject.AddComponent<TCPConnection>();
        udpClient = gameObject.AddComponent<UDPConnection>();

        tcpClient.Instantiate(hostIP, port);
        udpClient.Instantiate(hostIP, port);

        Debug.Log("Connected to " + hostIP + ":" + port);
    }

    // Run HandleData every game frame to ensure that commands are synchronized
    // with the game itself
    void FixedUpdate()
    {
        // Only clear out commands that arrived this frame
        int commandLength = commands.Count;
        for (int i = 0; i < commandLength; i++) {
            if (commands.Count <= 0) break;
            string command = commands.Dequeue();
            HandleData(command);
        }
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

    public int GetID()
    {
        return ID;
    }

    protected static void AddData(string message)
    {
        commands.Enqueue(message);
    }
}
