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
    protected static int ID = 0;
    public static Boolean isHost = false;
    private static NetworkController instance;
    TCPHost tcpHost;
    UDPHost udpHost;
    TCPConnection tcpClient;
    UDPConnection udpClient;
    
    [SerializeField]
    private GameObject mazeGeneratorObject;

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
        MazeGenerator mazeGenerator = mazeGeneratorObject.GetComponent<MazeGenerator>();

        // Prepare TCP and UDP host
        tcpHost.Instantiate(port);
        udpHost.Instantiate(port);

        isHost = true;

        // Create host's player object
        PlayerManager.CreateNewPlayer(ID);

        // Create maze
        mazeGenerator.Instantiate();

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

        // Create maze
        MazeGenerator mazeGenerator = mazeGeneratorObject.GetComponent<MazeGenerator>();
        mazeGenerator.Instantiate();

        Debug.Log("Connected to " + hostIP + ":" + port);
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
    protected void HandleData(string message)
    {
        // Parse incoming data, send to relevant managers
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

        object result = method.Invoke(instance, typedParams);
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
