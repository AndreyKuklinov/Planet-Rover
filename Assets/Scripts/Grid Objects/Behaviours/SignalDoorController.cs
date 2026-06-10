using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalDoorController : MonoBehaviour
{
    [SerializeField] Signal signal;
    [SerializeField] Door door;

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
        door.SetOpen(signalType == signal.SignalType);
    }
}
