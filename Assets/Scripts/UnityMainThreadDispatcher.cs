using System;
using System.Collections.Concurrent;
using UnityEngine;

/// <summary>
/// Used to route processing back to the main thread from sub-threads, in case something can only be called from the main thread for example.
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{

    private static UnityMainThreadDispatcher _instance;
    private static readonly ConcurrentQueue<Action> _executionQueue = new();

    public static UnityMainThreadDispatcher Instance()
    {
        if(_instance == null)
        {
            var obj = new GameObject("UnityMainThreadDispatcher");
            _instance = obj.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(obj);
        }

        return _instance;
    }

    private void Update()
    {
        while(_executionQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    /// <summary>
    /// Used to delegate computation to the main thread.
    /// </summary>
    /// <param name="action"> The code that should be run on the main thread. </param>
    public void Enqueue(Action action)
    {
        if(action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        _executionQueue.Enqueue(action);
    }

}