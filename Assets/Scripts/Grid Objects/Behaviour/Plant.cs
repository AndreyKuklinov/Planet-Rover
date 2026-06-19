using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Plant : MonoBehaviour, IGrabbable, IInteractable
{
    [SerializeField] ItemTypeData grownPlantData;
    [SerializeField] ItemTypeData ungrownPlantData;
    [SerializeField] Item item;
    [SerializeField] GridObject gridObject;
    
    public bool IsGrown
        => item.ItemData.TypeData == grownPlantData;

    public bool CanBeGrabbed =>
        IsGrown;

    public GridObject GridObject =>
        gridObject;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        if (IsGrown || grabbedObject == null 
            || !grabbedObject.GridObject.TryGetComponent<Bucket>(out var bucket))
            return false;

        return bucket.IsFilled && bucket.CurrentLiquid.ColorData == item.ColorData;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (!CanInteractWith(grabbedObject))
            throw new InvalidOperationException("Trying to interact with an object that can't interact with");

        var bucket = grabbedObject.GridObject.GetComponent<Bucket>();
        bucket.Empty();
        Grow();
        return grabbedObject;
    }

    [ContextMenu("Grow")]
    public void Grow()
    {
        SetGrown(true);
    }

    [ContextMenu("Ungrow")]
    public void Ungrow()
    {
        SetGrown(false);
    }

    private void SetGrown(bool value)
    {
        if (!IsSafeToChangeGrowthState()) 
            return;

        var newType = value ? grownPlantData : ungrownPlantData;
        item.ChangeData(newType, item.ItemData.ColorData);
    }

    private bool IsSafeToChangeGrowthState()
    {
        if (item == null || item.ItemData == null)
        {
            Debug.LogWarning("Cannot change state: 'Item' or 'Item.ItemData' is missing.", this);
            return false;
        }

        if (grownPlantData == null || ungrownPlantData == null)
        {
            Debug.LogWarning("Cannot change state: Plant Type Data (Grown/Ungrown) is missing in the Inspector.", this);
            return false;
        }

        return true;
    }
}
