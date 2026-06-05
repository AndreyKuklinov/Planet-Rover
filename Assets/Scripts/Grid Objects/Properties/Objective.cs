using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objective : MonoBehaviour
{
    [SerializeField] ObjectiveEventChannel objectiveCompleted;
    [SerializeField] ObjectiveEventChannel objectiveCreated;

    void Start()
    {
        objectiveCreated.Raise(this);
    }

    public void Complete()
    {
        objectiveCompleted.Raise(this);
    }
}
