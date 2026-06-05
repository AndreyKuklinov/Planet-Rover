using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalEmitter : MonoBehaviour
{
    public static event Action<SignalEmitter> EmitterSpawned;
    public static event Action<SignalEmitter> EmitterDestroyed;
    public static event Action<SignalType> SignalEmitted;

    public void Emit(SignalType signal)
    {
        SignalEmitted?.Invoke(signal);
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
