using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalDoorController : MonoBehaviour
{
    [SerializeField] ItemColorDataEventChannel roomSignalChanged;
    [SerializeField] Item item;
    [SerializeField] Door door;

    void Start()
    {
        roomSignalChanged.Raised += OnSignalChanged;
    }

    void OnDestroy()
    {
        roomSignalChanged.Raised -= OnSignalChanged;
    }

    private void OnSignalChanged(ItemColorData signal)
    {
        door.SetOpen(item.ColorData == signal);
    }
}
