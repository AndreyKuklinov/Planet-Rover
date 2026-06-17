using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] ItemCollector deliverableCollector;
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
