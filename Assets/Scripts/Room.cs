using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] RoomConfig config;
    public RoomConfig Config => config;
    public bool IsTimed => config == null || config.IsTimed;
    public bool CanUseReset => config == null || config.CanUseReset;
    [field: SerializeField, Min(0.01f)] public float StandardTime { get; private set; } = 30f;
    public static event Action<Room> RoomStarted;

    [SerializeField] RoomGrid grid;
    public RoomGrid Grid => grid;
    bool restoredFromSnapshot;

    public void PrepareForReset() => restoredFromSnapshot = true;
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
        if (restoredFromSnapshot) return;
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
