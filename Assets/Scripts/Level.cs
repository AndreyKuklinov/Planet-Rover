using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static event Action LevelCompleted;

    [SerializeField] LevelGrid grid;
    [SerializeField] Swapper swapper;

    readonly HashSet<Rocket> rockets = new();

    void Awake()
    {
        Rocket.RocketSpawned += OnRocketSpawned;
        Rocket.RocketCompleted += OnRocketCompleted;
        InitLevel();
    }

    void InitLevel()
    {
        swapper?.SwapAllObjects();
        grid.AttachAllObjects();
    }

    void OnDestroy()
    {
        Rocket.RocketCompleted -= OnRocketCompleted;
        Rocket.RocketSpawned -= OnRocketSpawned;
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
}
