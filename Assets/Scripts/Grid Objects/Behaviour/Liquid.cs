using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liquid : MonoBehaviour, IPassable, IInteractable
{
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LiquidData liquidData;

    public bool CanHandPassThrough =>
        true;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        if (grabbedObject == null)
            return false;

        if (!grabbedObject.GridObject.TryGetComponent<Bucket>(out var obj))
            return false;

        return true;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (!CanInteractWith(grabbedObject))
            throw new InvalidOperationException("Trying to interact with an object that can't interact with");

        var bucket = grabbedObject.GridObject.GetComponent<Bucket>();

        var data = bucket.CurrentLiquid;
        bucket.FillWith(liquidData);

        if (data == null)
        {
            Destroy(gameObject);
            return grabbedObject;
        }

        liquidData = data;
        spriteRenderer.sprite = liquidData.Sprite;
        return grabbedObject;

    }
}
