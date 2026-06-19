using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
    [SerializeField] ItemCollector deliverableCollector;
    [SerializeField] Objective objective;
    [SerializeField] Animator animator;
    [SerializeField] GridObject gridObject;

    void Start()
    {
        deliverableCollector.AllObjectsCollected += OnAllObjectsCollected;
    }

    private void OnAllObjectsCollected()
    {
        objective.Complete();
        animator.SetTrigger("Fulfilled");
        gridObject.TryRemoveFromGrid();
    }
}
