using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UDPHost : NetworkController
{
    private UdpClient udpServer;
    private HashSet<IPEndPoint> connectedClients;
    public static UDPHost instance;
    public bool isUDPPortActive { get; private set; }

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
            udpServer = new UdpClient(port);
            connectedClients = new HashSet<IPEndPoint>();

            udpServer.BeginReceive(OnReceiveData, null);
            Debug.Log("Hosting a UDP port");

            isUDPPortActive = true;
        }
        catch (SocketException)
        {
            Debug.Log("You are already hosting on another instance!");
        }
    }

    void FixedUpdate() {
        udpServer.BeginReceive(OnReceiveData, null);
    }

    void OnReceiveData(IAsyncResult result) {
        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] data = udpServer.EndReceive(result, ref clientEndPoint);
        if (!connectedClients.Contains(clientEndPoint)) {
            Debug.Log($"New client added: {clientEndPoint}");
            connectedClients.Add(clientEndPoint);
        }

        string message = Encoding.UTF8.GetString(data);
        // Debug.Log($"Received UDP message: {message}");

        AddData(message);
        SendDataToClients(message);
    }

    public void SendDataToClients(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (var client in connectedClients)
        {
            udpServer.Send(data, data.Length, client);
            // Debug.Log($"Sent to {client}: {message}");
        }
    }

    void OnApplicationQuit()
    {
        udpServer.Close();
    }
}
