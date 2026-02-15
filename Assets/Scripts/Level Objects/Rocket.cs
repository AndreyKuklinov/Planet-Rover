using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : LevelObject
{
    [field: SerializeField] public List<SampleData> RequiredSamples { get; private set; }

    public event Action RocketCompleted;
    public event Action RequiredObjectsChanged;

    override protected void Start()
    {
        RequiredObjectsChanged?.Invoke();
        base.Start();
    }

    public override bool CanReceive(LevelObject levelObject)
    {
        var sample = levelObject as Sample;
        if (sample == null)
            return false;
        return RequiredSamples.Contains(sample.Data);
    }

    public override void Receive(LevelObject levelObject)
    {
        var sample = levelObject as Sample;
        if (!CanReceive(sample))
            throw new ArgumentException("Can't drop " + levelObject + " on " + this);

        sample.Remove();
        RequiredSamples.Remove(sample.Data);
        RequiredObjectsChanged?.Invoke();
        CheckForCompletion();
    }

    void CheckForCompletion()
    {
        if (RequiredSamples.Count > 0)
            return;

        grid.RemoveObject(this);
        RocketCompleted?.Invoke();

        Destroy(gameObject);
    }
}
