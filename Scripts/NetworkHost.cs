using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class NetworkHost : MonoBehaviour
{
    public int port = 6077;
    private TcpListener listener;
    private TcpClient client;
    private NetworkStream stream;
    private Dictionary<int, TcpClient> connectedPeers = new Dictionary<int, TcpClient>();
    private Dictionary<int, NetworkStream> streams = new Dictionary<int, NetworkStream>();
    private Thread listenerThread;
    private PlayerManager playerManager;
    private int clientID = 0;
    private int nextID = 0;
    private byte[] inputBuffer = new byte[1024]; // Buffer for sending messages
    private byte[] outputBuffer = new byte[1024]; // Buffer for receiving messages
    private Boolean isHost = false;
    public static NetworkHost instance;
    
    // Singleton
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
        playerManager = FindObjectOfType<PlayerManager>();
    }

    /* 
    Begins a lobby as the host client in the P2P connection.
    Host client acts similar to a server.
    Handles keeping all clients synchronized.
    Host's ID is always 0.
    */
    public void StartAsHost()
    {
        try
        {
            // Get PlayerManager
            if (isHost) return;
            isHost = true;

            // Begin listening for other peers
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            // Create host's player object
            playerManager.CreateNewPlayer(clientID);

            // Make new thread that handles accepting client connections
            listenerThread = new Thread(() => AcceptClientConnections());
            listenerThread.Start();
        }
        catch (SocketException)
        {
            isHost = false;
            Debug.Log("You are already hosting on another instance!");
        }


    }

    /* 
    Connect to an already existing host as a client.
    Host sends client an ID to use for the session.
    */
    public void ConnectToHost(string hostIP)
    {
        if (isHost)
        {
            Debug.Log("You can't connect and host at the same time!");
        }
        client = new TcpClient(hostIP, port);
        connectedPeers.Add(0, client);
        NetworkStream stream = client.GetStream();
        streams.Add(0, stream);

        Debug.Log("Connected to host at IP: " + hostIP);

        // First message returned is the ID
        byte[] bufferID = new byte[4];
        int bytesRead = stream.Read(bufferID, 0, bufferID.Length);
        if (bytesRead == 4)
        {
            int receivedNumber = BitConverter.ToInt32(bufferID, 0);
            Debug.Log("Received ID: " + receivedNumber);
            clientID = receivedNumber;

            playerManager.CreateNewPlayer(clientID);
        }
        else
        {
            Debug.Log($"Error: 4 bytes were expected but {bytesRead} bytes were sent.");
        }

        // Begin accepting information from host
        Thread hostThread = new Thread(() => HandlePeer(stream, 0));
        hostThread.Start();
    }

    public void SendData(string message)
    {
        inputBuffer = Encoding.UTF8.GetBytes(message);
        foreach (KeyValuePair<int, NetworkStream> stream in streams)
        {
            stream.Value.Write(inputBuffer, 0, inputBuffer.Length);
            stream.Value.Flush();
        }
    }

    public void SendData(string message, int ignoreID)
    {
        inputBuffer = Encoding.UTF8.GetBytes(message);
        foreach (KeyValuePair<int, NetworkStream> stream in streams)
        {
            if (stream.Key == ignoreID) continue; // Don't send to same client
            stream.Value.Write(inputBuffer, 0, inputBuffer.Length);
            stream.Value.Flush();
        }
    }

    public int GetClientID()
    {
        return clientID;
    }

    public Boolean IsHost()
    {
        return isHost;
    }

    private void AcceptClientConnections()
    {
        Debug.Log("Preparing to accept new connections");
        while (true)
        {
            // Accept the connection
            TcpClient newClient = listener.AcceptTcpClient();
            Debug.Log("Peer connected!");

            InitializeClient(newClient);
        }
    }

    private void InitializeClient(TcpClient peerClient)
    {
        NetworkStream peerStream = peerClient.GetStream();

        // Assign an ID to the client
        nextID++;
        MainThreadDispatcher.Instance().Enqueue(() =>
        InitializeClientID(peerStream));
        Debug.Log($"Wrote client ID {nextID}");
        
        // Add to list of client connections
        connectedPeers.Add(nextID, peerClient);
        streams.Add(nextID, peerStream);

        // Create a player object in the host
        MainThreadDispatcher.Instance().Enqueue(() =>
        playerManager.CreateNewPlayer(nextID));
        Debug.Log("Created client on the host's end");

        // Send all current players and their positions to all clients
        MainThreadDispatcher.Instance().Enqueue(() =>
        SendData(playerManager.SendPlayerPositions()));
        Debug.Log("Sending player positions to all clients");

        // Spin a new thread that constantly updates using the peer's data
        Thread clientThread = new Thread(() => HandlePeer(peerStream, nextID));
        clientThread.Start();
    }

    private void InitializeClientID(NetworkStream peerStream)
    {

        inputBuffer = BitConverter.GetBytes(nextID);
        peerStream.Flush();
        peerStream.Write(inputBuffer, 0, inputBuffer.Length);
        peerStream.Flush();
    }

    private void HandlePeer(NetworkStream peerStream, int peerID)
    {
        byte[] peerBuffer = new byte[1024]; // Buffer for sending/receiving messages
        Debug.Log("Awaiting messages from peer");
        while (true)
        {
            int bytesRead = peerStream.Read(peerBuffer, 0, peerBuffer.Length);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(peerBuffer, 0, bytesRead);
            Debug.Log($"Received from peer {peerID}: " + message);

            // Handle received data
            // Use main thread as Unity doesn't allow API to be used on thread
            MainThreadDispatcher.Instance().Enqueue(() =>
            HandleData(message));

            // If host, send the message to all other peers (not including the one that sent it)
            if (IsHost())
            {
                SendData(message, peerID);
            }
        }
    }
    private void HandleData(string message)
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

    private void OnApplicationQuit()
    {
        listener?.Stop();
        client?.Close();
    }
}
