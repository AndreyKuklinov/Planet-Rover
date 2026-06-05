using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
    public bool CanBeGrabbed
        => true;

    public GridObject GridObject { get; }
}
