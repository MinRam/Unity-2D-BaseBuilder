using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue
{
    Queue<Job> jobQueue;

    Action<Job> cbJobCreated;

    public JobQueue(){
        jobQueue = new Queue<Job>();
    }

    public void Enqueue(Job j) {
        jobQueue.Enqueue(j);

        if (cbJobCreated != null) {
            cbJobCreated(j);
        }
    }

    public Job Dequeue() {
        if (jobQueue.Count <= 0) {
            return null;
        }
        return jobQueue.Dequeue();
    }

    public void RegisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated += cb;
    }
    public void UnRegisterJobCreationCallback(Action<Job> cb) {
        cbJobCreated -= cb;
    }
}
