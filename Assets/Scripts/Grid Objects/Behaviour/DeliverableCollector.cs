using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
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
            && grabbedObject.GridObject.TryGetComponent<Deliverable>(out var deliverable)
            && RequiredObjects.Contains(deliverable.DeliverableData);
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        var deliverable = grabbedObject.GridObject.GetComponent<Deliverable>();
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

        if(obj.TryGetComponent<IFillable>(out var fillable))
        {
            fillable.Empty();
            return grabbedObject;
        }

        Destroy(obj);
        return null;
    }
}
