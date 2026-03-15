using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelButton : LevelObject
{
    [SerializeField] SignalEmitter emitter;
    [SerializeField] Signal signal;

    public override bool CanReceive(LevelObject levelObject)
    {
        return levelObject == null;
    }

    public override void Receive(LevelObject levelObject)
    {
        Debug.Log(signal.ToString() + " button pressed!");
        emitter.SetSignals(signal);
    }
}
