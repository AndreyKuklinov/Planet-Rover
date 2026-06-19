using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalItemConverter : MonoBehaviour, IInteractable
{
    // TODO: Written in a hurry. Rewrite sensibly

    [SerializeField] Item item;
    [SerializeField] ItemColorDataEventChannel roomSignalChanged;
    [SerializeField] ItemTypeData inputItemType;
    [SerializeField] ItemTypeData outputItemType;

    public bool IsPowered { get; private set; } = false;

    void OnEnable()
    {
        roomSignalChanged.Raised += OnSignalChanged;
    }

    void OnDisable()
    {
        roomSignalChanged.Raised -= OnSignalChanged;
    }

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return grabbedObject != null
            && IsPowered
            && grabbedObject.GridObject.TryGetComponent<Item>(out var grabbedItem)
            && grabbedItem.ItemData.ColorData == item.ColorData
            && grabbedItem.ItemData.TypeData == inputItemType;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (!CanInteractWith(grabbedObject))
            throw new Exception(this + " can't interact with " + grabbedObject.GridObject);

        var grabbedItem = grabbedObject.GridObject.GetComponent<Item>();
        grabbedItem.ChangeData(outputItemType, grabbedItem.ColorData);
        return grabbedObject;
    }

    private void OnSignalChanged(ItemColorData signal)
    {
        IsPowered = signal == item.ColorData;
    }
}
