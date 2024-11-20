using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UDPConnection : NetworkController
{
    private UdpClient client;
    private IPEndPoint serverEndPoint;
    public static UDPConnection instance;

    // Singleton
    public void Instantiate(string hostIP, int port)
    {
        if (instance)
        {
            return;
        }

        // No instance yet, set this to it
        instance = this;

        client = new UdpClient();
        serverEndPoint = new IPEndPoint(IPAddress.Parse(hostIP), port);

        Debug.Log($"Connected to UDP port: {hostIP}");
        SendDataToHost("Connected");
        client.BeginReceive(OnReceiveData, null);
    }

    public void SendDataToHost(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        client.Send(data, data.Length, serverEndPoint);
    }

    void OnReceiveData(IAsyncResult result)
    {
        byte[] data = client.EndReceive(result, ref serverEndPoint);
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received UDP: {message}");
        MainThreadDispatcher.Instance().Enqueue(() => AddData(message));
        
        // Continue listening
        client.BeginReceive(OnReceiveData, null);
    }

    void OnApplicationQuit()
    {
        client.Close();
    }
}
