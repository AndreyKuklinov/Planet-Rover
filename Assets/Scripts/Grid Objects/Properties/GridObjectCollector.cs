using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class GridObjectCollector : MonoBehaviour, IGrabbableReceiver
{
    public event Action RequiredObjectsChanged;
    public event Action AllObjectsCollected;

    [field: SerializeField] public List<GridObjectData> RequiredObjects { get; private set; }

    void Start()
    {
        RequiredObjectsChanged?.Invoke();
    }

    public bool CanReceive(IGrabbable grabbedObject)
        => RequiredObjects.Contains(grabbedObject?.GridObject.Data);

    public IGrabbable Receive(IGrabbable grabbedObject)
    {
        Destroy(grabbedObject.GridObject.gameObject);
        RequiredObjects.Remove(grabbedObject.GridObject.Data);
        RequiredObjectsChanged?.Invoke();
        CheckForCompletion();

        return null;
    }

    void CheckForCompletion()
    {
        if (RequiredObjects.Count > 0)
            return;

        AllObjectsCollected?.Invoke();
    }
}
