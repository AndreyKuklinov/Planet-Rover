using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEmitter : MonoBehaviour, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SignalEmitter emitter;
    [SerializeField] Signal signal;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return true;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        emitter.Emit(signal.SignalType);
        return grabbedObject;
    }
}
