using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : MonoBehaviour, IPassable, IInteractable
{
    public bool CanHandPassThrough =>
        true;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        if (!grabbedObject.GridObject.TryGetComponent<IFillable>(out var obj))
            return false;

        return obj.CanContain(gameObject);
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if(!CanInteractWith(grabbedObject)) 
            throw new InvalidOperationException("Trying to interact with an object that can't interact with");

        var obj = grabbedObject.GridObject;
        var fillable = grabbedObject.GridObject.GetComponent<IFillable>();

        // TODO: implement
    }
}
