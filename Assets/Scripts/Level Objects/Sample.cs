using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : LevelObject 
{
    public enum SampleType
    {
        Red,
        Green
    }

    [field: SerializeField] public SampleType Type { get; private set; }

    public override bool CanBeGrabbed
        => true;
}
