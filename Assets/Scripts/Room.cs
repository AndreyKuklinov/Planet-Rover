using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Room : MonoBehaviour
{
    public static event Action<Room> RoomStarted;
    public static event Action RoomCompleted;
    public static event Action<SignalType> SignalChanged;

    [field: SerializeField] public int TimeBoostMultiplier { get; private set; } = 2;

    [SerializeField] ObjectiveEventChannel objectiveCreated;
    [SerializeField] ObjectiveEventChannel objectiveCompleted;
    [SerializeField] RoomGrid grid;
    [SerializeField] Swapper swapper;

    readonly HashSet<Objective> objectives = new();
    readonly HashSet<SignalEmitter> emitters = new();

    void Awake()
    {
        objectiveCreated.Raised += OnObjectiveCreated;
        objectiveCompleted.Raised += OnObjectiveCompleted;
        SignalEmitter.EmitterSpawned += OnEmitterSpawned;
        SignalEmitter.SignalEmitted += OnSignalChanged;
        SignalEmitter.EmitterDestroyed += OnEmitterDestroyed;
    }

    void OnDestroy()
    {
        objectiveCreated.Raised -= OnObjectiveCreated;
        objectiveCompleted.Raised -= OnObjectiveCompleted;
        SignalEmitter.EmitterSpawned -= OnEmitterSpawned;
        SignalEmitter.SignalEmitted -= OnSignalChanged;
        SignalEmitter.EmitterDestroyed -= OnEmitterDestroyed;
    }

    void Start()
    {
        InitLevel();
        RoomStarted?.Invoke(this);
    }

    void InitLevel()
    {
        if(swapper != null)
            swapper.SwapAllObjects();
        grid.AttachAllObjects();
    }

    private void OnObjectiveCompleted(Objective obj)
    {
        objectives.Remove(obj);
        if (objectives.Count == 0)
            RoomCompleted?.Invoke();
    }

    private void OnObjectiveCreated(Objective obj)
    {
        objectives.Add(obj);
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
}
