using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HandPassageMode
{
    InteractOrRetract,
    ContinueToFreeCell
}

public interface IPassable
{
    public bool CanHandPassThrough { get; }
    public HandPassageMode PassageMode { get; }
}
