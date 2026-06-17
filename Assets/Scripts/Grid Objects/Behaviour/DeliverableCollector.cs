using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverableCollector : MonoBehaviour, IInteractable
{
    public event Action RequiredObjectsChanged;
    public event Action AllObjectsCollected;

    [field: SerializeField] public List<DeliverableData> RequiredObjects { get; private set; }

    void Start()
    {
        RequiredObjectsChanged?.Invoke();
    }

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return grabbedObject != null
            && grabbedObject.GridObject.TryGetComponent<IDeliverable>(out var deliverable)
            && RequiredObjects.Contains(deliverable.DeliverableData);
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        var deliverable = grabbedObject.GridObject.GetComponent<IDeliverable>();
        RequiredObjects.Remove(deliverable.DeliverableData);
        RequiredObjectsChanged?.Invoke();
        CheckForCompletion();

        return GetReturnAfterDelivery(grabbedObject);
    }

    void CheckForCompletion()
    {
        if (RequiredObjects.Count > 0)
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
