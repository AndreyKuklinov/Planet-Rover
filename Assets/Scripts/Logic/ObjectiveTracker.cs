using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTracker : MonoBehaviour
{
    public event Action AllObjectivesWereFulfilled;

    [SerializeField] ObjectiveEventChannel objectiveCreated;
    [SerializeField] ObjectiveEventChannel objectiveCompleted;

    public HashSet<Objective> AllObjectives { get; private set; } = new HashSet<Objective>();
    public HashSet<Objective> FulfilledObjectives { get; private set; } = new HashSet<Objective>();

    public float FulfilledProportion
        => (float)(FulfilledObjectives.Count) / AllObjectives.Count;

    void Awake()
    {
        objectiveCreated.Raised += OnObjectiveCreated;
        objectiveCompleted.Raised += OnObjectiveCompleted;
    }

    private void OnDestroy()
    {
        objectiveCreated.Raised -= OnObjectiveCreated;
        objectiveCompleted.Raised -= OnObjectiveCompleted;
    }

    public void ClearObjectives()
    {
        AllObjectives.Clear();
        FulfilledObjectives.Clear();
    }

    private void OnObjectiveCreated(Objective objective)
    {
        AllObjectives.Add(objective);
    }

    private void OnObjectiveCompleted(Objective objective)
    {
        if (!AllObjectives.Contains(objective))
            throw new Exception("An objective was completed that was not registered by the objective tracker");

        FulfilledObjectives.Add(objective);
        if (FulfilledObjectives.Count == AllObjectives.Count)
            AllObjectivesWereFulfilled?.Invoke();
    }
}
