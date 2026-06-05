using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEmitter : GridObject, IGrabbableReceiver
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SignalEmitter emitter;
    [SerializeField] Signal signal;

    public bool CanReceive(IGrabbable grabbedObject)
    {
        return true;
    }

    public IGrabbable Receive(IGrabbable grabbedObject)
    {
        emitter.Emit(signal);
        return grabbedObject;
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
