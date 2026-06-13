using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;

    public int TotalNumberOfRooms
        => levelManager.CurrentLevel == null
            ? 0
            : levelManager.CurrentLevel.RoomCountToComplete;

    public int NumberOfCompletedRooms
        => levelManager.CompletedRoomCount;

    public int NumberOfRemainingRooms
        => TotalNumberOfRooms - NumberOfCompletedRooms;

    public float FinalScore
        => accumulatedScore / TotalNumberOfRooms;

    public float BestPossibleScore
        => (accumulatedScore + NumberOfRemainingRooms) / TotalNumberOfRooms;

    private float accumulatedScore; 

    private void OnEnable()
    {
        levelManager.LevelStarted += OnLevelStarted;
        levelManager.RoomFinished += OnRoomFinished;
        levelManager.LevelFinished += OnLevelFinished;
    }

    private void OnDisable()
    {
        levelManager.LevelStarted -= OnLevelStarted;
        levelManager.RoomFinished -= OnRoomFinished;
        levelManager.LevelFinished -= OnLevelFinished;
    }

    private void OnLevelStarted()
    {
        accumulatedScore = 0;
    }

    private void OnRoomFinished()
    {
        var proportion = levelManager.ObjectiveTracker.FulfilledProportion;
        accumulatedScore += proportion;
    }

    private void OnLevelFinished(LevelData levelData)
    {
        SaveDataManager.AddScore(levelData, FinalScore);
    }
}
