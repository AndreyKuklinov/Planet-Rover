using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static event Action LevelCompleted;
    public static event Action<Signal> SignalChanged;

    [SerializeField] LevelGrid grid;
    [SerializeField] Swapper swapper;

    readonly HashSet<Rocket> rockets = new();
    readonly HashSet<SignalEmitter> emitters = new();

    void Awake()
    {
        Rocket.RocketSpawned += OnRocketSpawned;
        Rocket.RocketCompleted += OnRocketCompleted;
        SignalEmitter.EmitterSpawned += OnEmitterSpawned;
        SignalEmitter.SignalEmitted += OnSignalChanged;
        SignalEmitter.EmitterDestroyed += OnEmitterDestroyed;
        InitLevel();
    }

    void InitLevel()
    {
        if(swapper != null)
            swapper.SwapAllObjects();
        grid.AttachAllObjects();
    }

    void OnDestroy()
    {
        Rocket.RocketCompleted -= OnRocketCompleted;
        Rocket.RocketSpawned -= OnRocketSpawned;
        SignalEmitter.EmitterSpawned -= OnEmitterSpawned;
        SignalEmitter.SignalEmitted -= OnSignalChanged;
        SignalEmitter.EmitterDestroyed -= OnEmitterDestroyed;
    }

    private void OnRocketCompleted(Rocket obj)
    {
        rockets.Remove(obj);
        if (rockets.Count == 0)
            LevelCompleted?.Invoke();
    }

    private void OnRocketSpawned(Rocket obj)
    {
        rockets.Add(obj);
    }

    private void OnSignalChanged(Signal _obj)
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
