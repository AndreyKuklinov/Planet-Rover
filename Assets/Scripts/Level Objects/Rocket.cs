using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : LevelObject
{
    public event Action RocketCompleted;

    [SerializeField] RocketData rocketData;

    public List<LevelObjectData> RequiredObjects = new();

    override protected void Start()
    {
        RequiredObjects = new List<LevelObjectData>(rocketData.RequiredObjects);
        base.Start();
    }

    public override bool CanBeDroppedOnThis(LevelObject levelObject)
        => levelObject != null && RequiredObjects.Contains(levelObject.Data);

    public override void DropOnThis(LevelObject levelObject)
    {
        if(!CanBeDroppedOnThis(levelObject))
            throw new ArgumentException("Can't drop " +  levelObject + " on " + this);

        Destroy(levelObject.gameObject);
        RequiredObjects.Remove(levelObject.Data);
        CheckForCompletion();
    }

    void CheckForCompletion()
    {
        if (RequiredObjects.Count > 0)
            return;

        grid.RemoveObject(this);
        RocketCompleted?.Invoke();

        Destroy(gameObject);
    }
}
