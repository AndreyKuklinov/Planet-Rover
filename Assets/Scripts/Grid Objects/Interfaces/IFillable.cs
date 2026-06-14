using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFillable
{
    public ContainableData CurrentContainedData { get; }
    public bool CanContain(ContainableData data);
    public void FillWith(ContainableData data);
    public void Empty();

    public bool IsFilled
        => CurrentContainedData != null;
}
