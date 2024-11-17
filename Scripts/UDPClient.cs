using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UDPClient : NetworkController
{
    private UDPClient client;
    private IPEndPoint serverEndPoint;

    // Singleton
    public void Instantiate(string hostIP)
    {
        if (instance)
        {
            // Remove if instance exists
            Destroy(gameObject);
            return;
        }

        // No instance yet, set this to it
        client = new UDPClient();
        serverEndPoint = new IPEndPoint(IPAddress.Loopback, port);

        udpClient.BeginReceive(OnReceiveData, null);
    }
    
    public void SendDataToHost(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, serverEndPoint);
    }

    void OnReceiveData(IAsyncResult result)
    {
        byte[] data = udpClient.EndReceive(result, ref serverEndPoint);
        string message = Encoding.UTF8.GetString(data);
        Debug.Log($"Received UDP: {message}");

        AddData(message);

        // Continue listening
        udpClient.BeginReceive(OnReceiveData, null);
    }

    void OnApplicationQuit()
    {
        udpClient.Close();
    }
}
