using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class Rocket : GridObject
{
    [SerializeField] GridObjectCollector gridObjectCollector;
    [SerializeField] Objective objective;

    void Start()
    {
        gridObjectCollector.AllObjectsCollected += OnAllObjectsCollected;
    }

    private void OnAllObjectsCollected()
    {
        objective.Complete();
        Destroy(gameObject);
    }
}
