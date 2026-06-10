using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventChannel<T> : ScriptableObject
{
    public event Action<T> Raised;
    
    public void Raise(T arg)
    {
        Raised?.Invoke(arg);
    }
}

