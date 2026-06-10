using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysGrabbable : MonoBehaviour, IGrabbable
{
    [SerializeField] GridObject gridObject;

    public GridObject GridObject =>
        gridObject;

    public bool CanBeGrabbed =>
        true;
}
