using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> executionQueue = new Queue<Action>();

    // Singleton instance of the dispatcher
    private static MainThreadDispatcher instance;

    void Awake()
    {
        instance = this;
    }
    
    public static MainThreadDispatcher Instance()
    {
        return instance;
    }

    // Enqueue an action to be executed on the main thread
    public void Enqueue(Action action)
    {
        // Lock while executing so other threads can't access
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    // Execute all actions in the queue on the main thread in the Update method
    private void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue()?.Invoke();
            }
        }
    }
}