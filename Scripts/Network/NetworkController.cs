using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class NetworkController : MonoBehaviour
{
    private static Queue<String> commands = new Queue<String>();
    public static int ID = 0;
    public static Boolean isHost = false;
    public static NetworkController instance;
    TCPHost tcpHost;
    UDPHost udpHost;
    TCPConnection tcpClient;
    UDPConnection udpClient;
    UDPLobbyConnection udpLobbyClient;
    public String username { get; private set; }

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
    }

    /* 
    Begins a lobby as the host client in the P2P connection.
    Host client acts similar to a Server.
    Handles keeping all clients synchronized.
    Host's ID is always 0.
    */
    public void HostGame(int port, string name)
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

        // Generate Seed
        RandomSeed.instance.InitializeSeed();
        username = name;
        ID = 0;

        Debug.Log("Hosted game!");
        SystemManager.instance.InitializeGame();
    }

    /* 
    Connect to an already existing host as a client.
    Host sends client an ID to use for the session.
    */
    public bool ConnectToGame(string hostIP, int port, string name)
    {
        // Set up TCP/UDP ports
        Debug.Log($"Using IP Address: {hostIP} and port {port}");

        if (isHost)
        {
            Debug.Log("Can't connect while hosting!");
            return false;
        }

        try
        {
            tcpClient = gameObject.AddComponent<TCPConnection>();
            udpClient = gameObject.AddComponent<UDPConnection>();

            tcpClient.Instantiate(hostIP, port);
            udpClient.Instantiate(hostIP, port);
        }
        catch (Exception)
        {
            tcpClient.Disconnect();
            udpClient.Disconnect();

            tcpClient = null;
            udpClient = null;
            Debug.LogWarning("Error connecting to host! Make sure you typed the IP address correctly.");
            return false;
        }

        // Generate Seed
        RandomSeed.instance.InitializeSeed();
        username = name;
        Debug.Log($"Connected to {hostIP}:{port}");
        SystemManager.instance.InitializeGame();
        UDPLobbyConnection.instance.Disconnect(); // No need to check for lobbies now
        return true; // Returns true if connecting was successful
    }

    public void StartGame()
    {
        if (isHost)
        {
            // Start sending player positions to clients
            PlayerManager.instance.BeginSendingHostPositionsToClients();
            EnemyManager.instance.BeginSendingHostPositionsToClients();
            ItemManager.instance.BeginSendingHostPositionsToClients();

            UDPHost.instance.StopBroadcasting();
        }
        else
        {
            // Ask host to create player and for usernames
            TCPConnection.instance.SendDataToHost($"PlayerManager:CreateNewPlayer:{ID}:{username}");
            PlayerManager.RequestPlayerUsernames();
        }

        SystemManager.instance.StartGame(username);
    }

    public void SearchForGames()
    {
        if (udpLobbyClient == null)
        {
            udpLobbyClient = gameObject.AddComponent<UDPLobbyConnection>();
            udpLobbyClient.Instantiate(6077);
        }
        
        udpLobbyClient.RequestAddressFromHosts();
    }

    public void DisconnectFromGame()
    {
        Debug.Log("Running disconnect from game");
        if (isHost)
        {
            // Prepare TCP and UDP host
            tcpHost.Disconnect();
            udpHost.Disconnect();

            tcpHost = null;
            udpHost = null;
        }
        else
        {
            tcpClient.DisconnectFromHost();
            udpClient.Disconnect();

            tcpClient = null;
            udpClient = null;
        }

        isHost = false;
        username = null;
        SystemManager.instance.Reset();

        Debug.Log("Finished");
    }

    void FixedUpdate()
    {
        // Only clear out commands that arrived this frame
        int commandLength = commands.Count;
        for (int i = 0; i < commandLength; i++)
        {
            if (commands.Count <= 0) break;
            string command = commands.Dequeue();
            HandleData(command);
        }
    }

    // Run HandleData every game frame to ensure that commands are synchronized
    // with the game itself
    // HandleData takes a message and converts it into a static method.
    void HandleData(string message)
    {
        // Parse incoming data, send to relevant managers
        try
        {
            string[] data = message.Split(':');
            if (data.Length < 2)
            {
                Debug.Log("Invalid command format.");
                return;
            }

            Type type = Type.GetType(data[0]);
            if (type == null)
            {
                Debug.Log($"Class type '{data[0]}' could not be found.");
                return;
            }

            MethodInfo method = type.GetMethod(data[1]);
            if (method == null)
            {
                Debug.Log($"Method type '{data[1]}' could not be found.");
                return;
            }

            ParameterInfo[] methodParams = method.GetParameters();
            object[] typedParams = new object[methodParams.Length];
            for (int i = 2; i < data.Length; i++)
            {
                // Convert the rest into parameters to use for the method
                if (i >= methodParams.Length + 2)
                {
                    Debug.Log("Insufficient parameters provided for the method.");
                    return;
                }

                // Convert parameter string to the appropriate type
                typedParams[i - 2] = Convert.ChangeType(data[i], methodParams[i - 2].ParameterType);
            }

            try
            {
                object result = method.Invoke(instance, typedParams);
            }
            catch (TargetException)
            {
                Debug.Log($"Error: could not invoke method passed in: {message}");
            }
        }
        catch (NullReferenceException)
        {
            Debug.LogWarning($"Couldn't parse the message: {message}");
        }


    }

    public static void AddData(string message)
    {
        commands.Enqueue(message);
    }

    public static void RemovePlayer(int ID)
    {
        PlayerManager.instance.RemovePlayer(ID);
        if (isHost)
        {
            TCPHost.instance.SendDataToClients($"NetworkController:RemovePlayer:{ID}");
        }
    }
}
