using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class DeliverableCollector : MonoBehaviour, IGrabbableReceiver
{
    public event Action RequiredObjectsChanged;
    public event Action AllObjectsCollected;

    [field: SerializeField] public List<DeliverableData> RequiredObjects { get; private set; }

    void Start()
    {
        RequiredObjectsChanged?.Invoke();
    }

    public bool CanReceive(IGrabbable grabbedObject)
    {
        return grabbedObject != null
            && grabbedObject.GridObject.TryGetComponent<Deliverable>(out var deliverable)
            && RequiredObjects.Contains(deliverable.DeliverableData);
    }

    public IGrabbable Receive(IGrabbable grabbedObject)
    {
        var deliverable = grabbedObject.GridObject.GetComponent<Deliverable>();
        RequiredObjects.Remove(deliverable.DeliverableData);
        RequiredObjectsChanged?.Invoke();
        CheckForCompletion();

        Destroy(grabbedObject.GridObject.gameObject);
        return null;
    }

    void CheckForCompletion()
    {
        if (RequiredObjects.Count > 0)
            return;

        AllObjectsCollected?.Invoke();
    }
}
