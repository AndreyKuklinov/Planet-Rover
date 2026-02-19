using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Level : MonoBehaviour
{
    public static event Action LevelCompleted;

    [SerializeField] LevelGrid grid;

    HashSet<Rocket> rockets = new HashSet<Rocket>();

    void Awake()
    {
        Rocket.RocketSpawned += OnRocketSpawned;
        Rocket.RocketCompleted += OnRocketCompleted;
    }

    private void OnRocketCompleted(Rocket obj)
    {
        rockets.Remove(obj);
        if (rockets.Count == 0)
            LevelCompleted?.Invoke();
    }

    private void OnRocketSpawned(Rocket obj)
    {
        Debug.Log("what");
        rockets.Add(obj);
        Debug.Log(rockets.Count);
    }
}
