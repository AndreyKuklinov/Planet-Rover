using System;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Cloud : MonoBehaviour, IInteractable, IPassable
{
    [SerializeField] ItemData itemData;

    public ItemColorData ColorData
        => itemData.ColorData;

    public bool CanHandPassThrough 
        => true;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        if (grabbedObject == null 
            || !grabbedObject.GridObject.TryGetComponent<IRecolorable>(out var recolorable))
            return false;

        return recolorable.CanBeRecolored(ColorData) && recolorable.CurrentColor != ColorData;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (!CanInteractWith(grabbedObject))
            throw new InvalidOperationException("Trying to interact with an object that can't interact with");

        var recolorable = grabbedObject.GridObject.GetComponent<IRecolorable>();
        recolorable.Recolor(ColorData);
        Destroy(gameObject);
        return grabbedObject;
    }
}