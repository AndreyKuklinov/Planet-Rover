using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] DeliverableCollector deliverableCollector;
    [SerializeField] Objective objective;

    void Start()
    {
        deliverableCollector.AllObjectsCollected += OnAllObjectsCollected;
    }

    private void OnAllObjectsCollected()
    {
        objective.Complete();
        Destroy(gameObject);
    }
}
