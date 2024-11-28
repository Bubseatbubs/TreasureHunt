using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TCPHost : MonoBehaviour
{
    private TcpListener listener;
    private Dictionary<int, TcpClient> connectedPeers = new Dictionary<int, TcpClient>();
    private Dictionary<int, NetworkStream> streams = new Dictionary<int, NetworkStream>();
    private byte[] inputBuffer = new byte[1024];
    private Thread listenerThread;
    private int nextID = 0;
    public static TCPHost instance;

    /* 
    Begins a lobby as the host client in the P2P connection.
    Host client acts similar to a server.
    Handles keeping all clients synchronized.
    Host's ID is always 0.
    */
    public void Instantiate(int port)
    {
        try
        {
            if (instance)
            {
                return;
            }

            // No instance yet, set this to it
            instance = this;

            // Begin listening for other peers
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Debug.Log("Hosting a TCP port");

            // Make new thread that handles accepting client connections
            listenerThread = new Thread(() => AcceptClientConnections());
            listenerThread.Start();
        }
        catch (SocketException)
        {
            Debug.Log("You are already hosting on another instance!");
        }
    }

    public void SendDataToClients(string message)
    {
        inputBuffer = Encoding.UTF8.GetBytes(message);
        foreach (KeyValuePair<int, NetworkStream> stream in streams)
        {
            stream.Value.Write(inputBuffer, 0, inputBuffer.Length);
            stream.Value.Flush();
        }
    }

    public void SendDataToClients(string message, int ignoreID)
    {
        inputBuffer = Encoding.UTF8.GetBytes(message);
        foreach (KeyValuePair<int, NetworkStream> stream in streams)
        {
            if (stream.Key == ignoreID) continue; // Don't send to same client
            stream.Value.Write(inputBuffer, 0, inputBuffer.Length);
            stream.Value.Flush();
        }
    }

    public void RemoveClient(int ID)
    {
        connectedPeers.Remove(ID);
        streams.Remove(ID);
        Debug.Log($"Removed client {ID}'s TCP connection");
    }

    private void AcceptClientConnections()
    {
        Debug.Log("Preparing to accept new connections");
        while (true)
        {
            // Accept the connection
            TcpClient newClient = listener.AcceptTcpClient();
            Debug.Log("Peer connected to TCP Port!");

            InitializeClient(newClient);
        }
    }

    private void InitializeClient(TcpClient peerClient)
    {
        NetworkStream peerStream = peerClient.GetStream();

        // Assign an ID to the client
        nextID++;
        MainThreadDispatcher.instance.Enqueue(() =>
        InitializeClientID(peerStream));
        Debug.Log($"Wrote client ID {nextID}");

        // Add to list of client connections
        connectedPeers.Add(nextID, peerClient);
        streams.Add(nextID, peerStream);

        // Spin a new thread that constantly updates using the peer's data
        Thread clientThread = new Thread(() => HandlePeer(peerStream, nextID));
        clientThread.Start();
    }

    private void InitializeClientID(NetworkStream peerStream)
    {
        inputBuffer = BitConverter.GetBytes(nextID);
        peerStream.Write(inputBuffer, 0, inputBuffer.Length);
        peerStream.Flush();
    }

    private void HandlePeer(NetworkStream peerStream, int peerID)
    {
        byte[] peerBuffer = new byte[4096];
        Debug.Log($"Awaiting messages from peer {peerID}");
        try
        {
            while (true)
            {
                int bytesRead = peerStream.Read(peerBuffer, 0, peerBuffer.Length);
                if (bytesRead == 0) break;

                string message = Encoding.UTF8.GetString(peerBuffer, 0, bytesRead);
                // Debug.Log($"Received from peer {peerID}: " + message);

                if (message.Equals("TCP:Disconnect")) {
                    throw new Exception();
                }

                NetworkController.AddData(message);
            }
        }
        catch (Exception e)
        {
            // If client disconnects or some other error occurs while reading
            Debug.Log($"Client {peerID} disconnected because of: {e.Message}");
            RemoveClient(peerID);
            NetworkController.RemovePlayer(peerID);
        }

    }

    private void OnApplicationQuit()
    {
        listener?.Stop();
    }
}
