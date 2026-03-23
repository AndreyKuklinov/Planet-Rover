using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEmitter : LevelObject
{
    [SerializeField] SignalEmitter emitter;
    [SerializeField] Signal signal;

    public override bool CanReceive(LevelObject levelObject)
    {
        return levelObject == null;
    }

    public override void Receive(LevelObject levelObject)
    {
        emitter.Emit(signal);
    }

    private void OnValidate()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        spriteRenderer.color = SignalColor.GetColor(signal);
    }
}
