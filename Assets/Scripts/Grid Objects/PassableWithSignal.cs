using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassableWithSignal : MonoBehaviour, IPassable
{
    [SerializeField] Signal signal;
    private bool isOpen;

    public bool CanHandPassThrough
        => isOpen;

    void Start()
    {
        Room.SignalChanged += OnSignalChanged;
    }

    void OnDestroy()
    {
        Room.SignalChanged -= OnSignalChanged;
    }

    private void OnSignalChanged(SignalType signalType)
    {
        isOpen = signal.SignalType == signalType;
    }
}
