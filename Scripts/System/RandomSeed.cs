using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;

/*
Unity has a limitation on threads. Nothing related to the Unity API can be run
on a thread. As such, the purpose of this class is for threads to make calls to
the main thread to run a specific function that uses the Unity API.
*/
public class RandomSeed : MonoBehaviour
{
    public int _seed {get; private set;}

    // Singleton instance of the dispatcher
    public static RandomSeed instance;

    void Awake()
    {
        instance = this;
    }

    // Generates a seed if host, or gets seed from host if client
    public void InitializeSeed() {
        if (NetworkController.isHost)
        {
            _seed = Random.Range(1000000, 9999999);
            Debug.Log($"Seed: {_seed}");
            Random.InitState(_seed);
        }
        else
        {
            _seed = RequestSeed();

            Debug.Log($"Seed: {_seed}");
            Random.InitState(_seed);
        }
    }

    int RequestSeed()
    {
        string message = TCPConnection.instance.SendAndReceiveDataFromHost("RandomSeed:SendSeed");
        return int.Parse(message);
    }

    public static void SendSeed()
    {
        TCPHost.instance.SendDataToClients($"{instance._seed}");
    }
}