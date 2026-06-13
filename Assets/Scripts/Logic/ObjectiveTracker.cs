using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveTracker : MonoBehaviour
{
    public event Action AllObjectivesWereFulfilled;

    [SerializeField] ObjectiveEventChannel objectiveCreated;
    [SerializeField] ObjectiveEventChannel objectiveCompleted;
    [SerializeField] RoomLoader roomLoader;

    public HashSet<Objective> AllObjectives { get; private set; } = new HashSet<Objective>();
    public HashSet<Objective> FulfilledObjectives { get; private set; } = new HashSet<Objective>();

    public float FulfilledProportion
        => AllObjectives.Count == 0 
            ? 1f
            : (float)(FulfilledObjectives.Count) / AllObjectives.Count;

    void Awake()
    {
        objectiveCreated.Raised += OnObjectiveCreated;
        objectiveCompleted.Raised += OnObjectiveCompleted;
        roomLoader.LoadedRoom += OnRoomloaded;
        DebugObjectives();
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
        //Debug.Log("Objectives cleared");
        //DebugObjectives();
    }

    private void OnObjectiveCreated(Objective objective)
    {
        AllObjectives.Add(objective);

        //DebugObjectives();
    }

    private void OnObjectiveCompleted(Objective objective)
    {
        if (!AllObjectives.Contains(objective))
            throw new Exception("An objective was completed that was not registered by the objective tracker");

        FulfilledObjectives.Add(objective);
        if (FulfilledObjectives.Count == AllObjectives.Count)
            AllObjectivesWereFulfilled?.Invoke();

        //DebugObjectives();
    }

    private void DebugObjectives()
    {
        Debug.Log(FulfilledObjectives.Count + "/" + AllObjectives.Count);
    }

    private void OnRoomloaded()
    {
        ClearObjectives();
    }
}
