using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class TCPConnection : NetworkController
{
    private TcpClient host;
    private NetworkStream hostStream;
    public static TCPConnection instance;

    // Singleton
    public void Instantiate(string hostIP, int port)
    {
        if (instance)
        {
            Debug.Log("TCPConnection Instance already exists");
            return;
        }

        // No instance yet, set this to it
        instance = this;

        // Initialize client
        host = new TcpClient(hostIP, port);
        hostStream = host.GetStream();
        Debug.Log("Connected to TCP host at IP: " + hostIP);
        
        // First message returned is the ID
        byte[] bufferID = new byte[4];
        int bytesRead = hostStream.Read(bufferID, 0, bufferID.Length);
        if (bytesRead == 4)
        {
            int receivedNumber = BitConverter.ToInt32(bufferID, 0);
            Debug.Log("Client's ID is now: " + receivedNumber);
            ID = receivedNumber;

            PlayerManager.instance.CreateNewPlayer(ID);
        }
        else
        {
            Debug.Log($"Error: 4 bytes were expected but {bytesRead} bytes were sent.");
        }

        // Begin accepting information from host
        Thread hostThread = new Thread(() => HandleHost(hostStream, 0));
        hostThread.Start();
    }

    private void HandleHost(NetworkStream peerStream, int peerID)
    {
        byte[] peerBuffer = new byte[4096];
        Debug.Log("Awaiting messages from peer");
        while (true)
        {
            int bytesRead = peerStream.Read(peerBuffer, 0, peerBuffer.Length);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(peerBuffer, 0, bytesRead);
            Debug.Log($"Received TCP message from peer {peerID}: " + message);

            // Handle received data
            // Use main thread as Unity doesn't allow API to be used on thread
            AddData(message);
        }
    }

    public void SendDataToHost(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        hostStream.Write(data, 0, data.Length);
        hostStream.Flush();
    }

    private void OnApplicationQuit()
    {
        host?.Close();
    }
}
