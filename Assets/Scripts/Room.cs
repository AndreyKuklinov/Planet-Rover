using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    public static event Action<Room> RoomStarted;
    public static event Action AllObjectivesCompleted;
    public static event Action<SignalType> SignalChanged;

    [field: SerializeField] public int TimeBoostMultiplier { get; private set; } = 2;

    [SerializeField] RoomGrid grid;
    [SerializeField] Swapper swapper;
    [field: SerializeField] public ObjectiveTracker ObjectiveTracker { get; private set; }

    readonly HashSet<SignalEmitter> emitters = new();

    void Awake()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled += OnAllObjectivesFulfilled;
        SignalEmitter.EmitterSpawned += OnEmitterSpawned;
        SignalEmitter.SignalEmitted += OnSignalChanged;
        SignalEmitter.EmitterDestroyed += OnEmitterDestroyed;
    }

    void OnDestroy()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled -= OnAllObjectivesFulfilled;
        SignalEmitter.EmitterSpawned -= OnEmitterSpawned;
        SignalEmitter.SignalEmitted -= OnSignalChanged;
        SignalEmitter.EmitterDestroyed -= OnEmitterDestroyed;
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

    private void OnSignalChanged(SignalType _obj)
    {
        SignalChanged?.Invoke(_obj);
    }

    private void OnEmitterSpawned(SignalEmitter obj)
    {
        emitters.Add(obj);
    }

    private void OnEmitterDestroyed(SignalEmitter obj)
    {
        emitters.Remove(obj);
    }

    private void OnAllObjectivesFulfilled()
    {
        AllObjectivesCompleted?.Invoke();
    }
}
