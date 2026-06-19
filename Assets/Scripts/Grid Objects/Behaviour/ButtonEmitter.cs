using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonEmitter : MonoBehaviour, IInteractable
{
    [SerializeField] Item item;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] SignalEmitter emitter;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return !item.ColorData.IsNeutral;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        emitter.Emit(item.ColorData);
        return grabbedObject;
    }
}
