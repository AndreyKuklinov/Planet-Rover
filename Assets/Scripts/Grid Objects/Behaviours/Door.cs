using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IPassable
{
    [field: SerializeField] public bool IsOpen { get; private set; } = false;

    public bool CanHandPassThrough =>
        IsOpen;

    public void SetOpen(bool isOpen)
    {
        IsOpen = isOpen;
    }
}
