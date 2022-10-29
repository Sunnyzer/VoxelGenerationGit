using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ThreadManager : Singleton<ThreadManager>
{
    [SerializeField] List<Thread> threadsQueue = new List<Thread>();
    [SerializeField] List<Thread> threadsRunning = new List<Thread>();
    [SerializeField] int simultanousThread = 1;
    [SerializeField] int threadRunningCount = 0;
    [SerializeField] int threadQueueCount = 0;

    public bool IsEmptyThreadsRunning => threadsRunning.Count == 0;
    public bool IsEmptyThreads => threadsRunning.Count == 0 && threadsQueue.Count == 0;

    void ThreadUpdate()
    {
        threadRunningCount = threadsRunning.Count;
        threadQueueCount = threadsQueue.Count;
        for (int i = 0; i < threadsRunning.Count;)
        {
            if (threadsRunning[i].ThreadState != ThreadState.Running)
            {
                threadsRunning[i].Abort();
                threadsRunning.RemoveAt(i);
                if (threadsQueue.Count == 0) continue;
                threadsQueue[0].Start();
                threadsRunning.Add(threadsQueue[0]);
                threadsQueue.RemoveAt(0);
                continue;
            }
            i++;
        }
    }
    private void Update() => ThreadUpdate();
    public void AddThread(Action _action)
    {
        Thread _thread = new Thread(new ThreadStart(_action));
        if(threadsRunning.Count < simultanousThread)
        {
            _thread.Start();
            threadsRunning.Add(_thread);
        }
        else
        {
            threadsQueue.Add(_thread);
        }
    }
}
