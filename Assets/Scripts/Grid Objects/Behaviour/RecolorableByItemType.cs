using System;
using UnityEngine;

public class RecolorableByItemType : MonoBehaviour, IInteractable
{
    [SerializeField] Item item;
    [SerializeField] ItemTypeData recoloringItemType;
    [SerializeField] bool consumesItem = true;
    [SerializeField] bool requiresNeutralColor = true;
        
    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        if (requiresNeutralColor && !item.ColorData.IsNeutral)
            return false;

        return grabbedObject != null
            && grabbedObject.GridObject.TryGetComponent<IHasItemData>(out var grabbedItem)
            && grabbedItem.ItemData.ColorData != null;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (!CanInteractWith(grabbedObject))
            throw new Exception(this + " can't interact with " + grabbedObject.GridObject);

        var grabbedItem = grabbedObject.GridObject.GetComponent<IHasItemData>();
        item.Recolor(grabbedItem.ItemData.ColorData);

        if (consumesItem)
        {
            Destroy(grabbedObject.GridObject.gameObject);
            return null;
        }

        return grabbedObject;
    }
}