using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class Rocket : GridObject, IGrabbableReceiver, IObjective
{
    [field: SerializeField] public List<SampleData> RequiredSamples { get; private set; }

    public bool IsFinal
        => false;

    public static event Action<SampleData> SampleDelivered;
    public event Action RequiredObjectsChanged;

    [SerializeField] ObjectiveEventChannel objectiveCompleted;
    [SerializeField] ObjectiveEventChannel objectiveCreated;

    void Start()
    {
        RequiredObjectsChanged?.Invoke();
        objectiveCreated.Raise(this);
    }

    void CheckForCompletion()
    {
        if (RequiredSamples.Count > 0)
            return;

        RoomGrid.RemoveObject(this);
        objectiveCompleted.Raise(this);

        Destroy(gameObject);
    }

    public void CompleteObjective()
    {
        throw new NotImplementedException();
    }

    public bool CanReceive(IGrabbable grabbedObject)
    {
        var sample = grabbedObject as SolidSample;
        if (sample == null)
            return false;
        return RequiredSamples.Contains(sample.SampleData);
    }

    public IGrabbable Receive(IGrabbable grabbedObject)
    {
        var sample = grabbedObject as SolidSample;
        if (sample == null)
            throw new InvalidOperationException(grabbedObject 
                + " is not a solid sample and can't be dropped on " + this);

        Destroy(sample.gameObject);
        RequiredSamples.Remove(sample.SampleData);
        SampleDelivered?.Invoke(sample.SampleData);
        RequiredObjectsChanged?.Invoke();
        CheckForCompletion();

        return null;
    }
}
