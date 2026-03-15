using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalEmitter : MonoBehaviour
{
    public event Action<Signal[]> SignalsChanged;

    public Signal[] CurrentSignals { get; private set; }

    public void SetSignals(params Signal[] signals)
    {
        CurrentSignals = signals;
        SignalsChanged?.Invoke(signals);
    }
}
