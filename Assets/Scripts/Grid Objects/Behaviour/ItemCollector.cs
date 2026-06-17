using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollector : MonoBehaviour, IInteractable
{
    public event Action RequiredObjectsChanged;
    public event Action AllObjectsCollected;

    [field: SerializeField] public List<ItemData> RequiredItems { get; private set; }

    void Start()
    {
        RequiredObjectsChanged?.Invoke();
    }

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return grabbedObject != null
            && grabbedObject.GridObject.TryGetComponent<IHasItemData>(out var deliverable)
            && RequiredItems.Contains(deliverable.ItemData);
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        var deliverable = grabbedObject.GridObject.GetComponent<IHasItemData>();
        RequiredItems.Remove(deliverable.ItemData);
        RequiredObjectsChanged?.Invoke();
        CheckForCompletion();

        return GetReturnAfterDelivery(grabbedObject);
    }

    void CheckForCompletion()
    {
        if (RequiredItems.Count > 0)
            return;

        AllObjectsCollected?.Invoke();
    }

    IGrabbable GetReturnAfterDelivery(IGrabbable grabbedObject)
    {
        var obj = grabbedObject.GridObject.gameObject;

        if(obj.TryGetComponent<Bucket>(out var bucket) && bucket.IsFilled)
        {
            bucket.Empty();
            return grabbedObject;
        }

        Destroy(obj);
        return null;
    }
}
