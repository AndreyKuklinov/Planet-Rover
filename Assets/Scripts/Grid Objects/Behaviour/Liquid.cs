using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : MonoBehaviour, IPassable, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ContainableData containableData;

    public bool CanHandPassThrough =>
        true;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        if (grabbedObject == null)
            return false;

        if (!grabbedObject.GridObject.TryGetComponent<IFillable>(out var obj))
            return false;

        return obj.CanContain(containableData);
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (!CanInteractWith(grabbedObject))
            throw new InvalidOperationException("Trying to interact with an object that can't interact with");

        var fillable = grabbedObject.GridObject.GetComponent<IFillable>();

        var data = fillable.CurrentContainedData;
        fillable.FillWith(containableData);

        if (data == null)
        {
            Destroy(gameObject);
            return grabbedObject;
        }

        containableData = data;
        spriteRenderer.sprite = containableData.Sprite;
        return grabbedObject;

    }
}
