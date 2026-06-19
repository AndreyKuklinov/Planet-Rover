using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalEmitter : MonoBehaviour
{
    [SerializeField] ItemColorDataEventChannel signalEmitted;

    public void Emit(ItemColorData signal)
    {
        if (signal.IsNeutral)
            return;

        signalEmitted.Raise(signal);
    }
}
