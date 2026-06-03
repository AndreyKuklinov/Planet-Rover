using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public event Action TimeOut;

    public float TimeRemaining { get; private set; }
    public float Duration { get; private set; }
    public bool IsTimeRunning { get; private set;  }

    public void StartTime(float duration)
    {
        Duration = duration;
        IsTimeRunning = true;
        TimeRemaining = duration;
    }

    void Update()
    {
        if (!IsTimeRunning)
            return;

        TimeRemaining -= Time.deltaTime;

        if( TimeRemaining <= 0 )
        {
            IsTimeRunning = false;
            TimeRemaining = 0;
            TimeOut?.Invoke();
        }
    }
}
