using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalEmitter : MonoBehaviour
{
    [SerializeField] ItemColorDataEventChannel signalEmitted;

    public void Emit(ItemColorData signal)
    {
        signalEmitted.Raise(signal);
    }

    void Start()
    {
        //EmitterSpawned?.Invoke(this);
    }

    private void OnDestroy()
    {
        //EmitterDestroyed?.Invoke(this);
    }
}
