using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalEmitter : MonoBehaviour
{
    public static event Action<SignalEmitter> EmitterSpawned;
    public static event Action<SignalEmitter> EmitterDestroyed;
    public static event Action<Signal[]> SignalsChanged;

    public Signal[] ActiveSignals { get; private set; } = new Signal[0];

    public void SetSignals(params Signal[] signals)
    {
        ActiveSignals = signals;
        SignalsChanged?.Invoke(signals);
    }

    void Start()
    {
        EmitterSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        EmitterDestroyed?.Invoke(this);
    }
}
