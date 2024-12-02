using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UDPLobbyConnection : MonoBehaviour
{
    private UdpClient lobbyClient;
    private IPEndPoint broadcastEndPoint;
    public static UDPLobbyConnection instance;
    private bool isUDPPortActive = false;

    // Singleton
    public void Instantiate(string hostIP, int port)
    {
        if (instance)
        {
            return;
        }

        // No instance yet, set this to it
        instance = this;

        lobbyClient = new UdpClient();
        lobbyClient.EnableBroadcast = true;
        broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, port);
    }

    public void Disconnect()
    {
        lobbyClient?.Close();
        instance = null;
        Destroy(this);
    }

    public void RequestAddressFromHosts()
    {
        String command = "UDP_TreasureHunt:RequestAddress";
        byte[] data = Encoding.UTF8.GetBytes(command);
        lobbyClient.Send(data, data.Length, broadcastEndPoint);
    }

    void FixedUpdate()
    {
        if (isUDPPortActive)
        {
            lobbyClient.BeginReceive(OnReceiveData, null);
        }
    }

    void OnReceiveData(IAsyncResult result)
    {
        byte[] data = lobbyClient.EndReceive(result, ref broadcastEndPoint);
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received UDP: {message}");
        NetworkController.AddData(message);
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
}
