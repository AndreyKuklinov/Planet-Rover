using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    public static event Action<Room> RoomStarted;

    [SerializeField] RoomGrid grid;
    [SerializeField] Swapper swapper;
    [SerializeField] ItemColorDataEventChannel signalEmitted;
    [SerializeField] ItemColorDataEventChannel roomSignalChanged;

    void Awake()
    {
        signalEmitted.Raised += OnSignalEmitted;
    }

    void OnDestroy()
    {
        signalEmitted.Raised -= OnSignalEmitted;
    }

    void Start()
    {
        InitRoom();
        RoomStarted?.Invoke(this);
    }

    void InitRoom()
    {
        if(swapper != null)
            swapper.SwapAllObjects();
        grid.AttachAllObjects();
    }

    private void OnSignalEmitted(ItemColorData obj)
    {
        roomSignalChanged.Raise(obj);
    }
}
