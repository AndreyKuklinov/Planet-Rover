using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public event Action<LevelData> LevelFinished;
    public event Action LevelStarted;
    public event Action RoomFinished;

    public LevelData CurrentLevel {  get; private set; }
    public int CompletedRoomCount { get; private set; }

    [SerializeField] LevelLoader levelLoader;
    [field: SerializeField] public ObjectiveTracker ObjectiveTracker { get; private set; }
    [SerializeField] Timer timer;

    public bool IsLevelRunning
        => CurrentLevel != null;

    void OnEnable()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled += OnAllObjectivesInRoomCompleted;
        timer.TimeOut += OnTimeOut;
    }

    void OnDisable()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled -= OnAllObjectivesInRoomCompleted;
        timer.TimeOut -= OnTimeOut;
    }

    public void StartLevel(LevelData levelData)
    {
        CurrentLevel = levelData;
        CompletedRoomCount = 0;
        levelLoader.SetLevelData(levelData);
        LevelStarted?.Invoke();
        StartNextRoom();
    }

    private void StartNextRoom()
    {
        levelLoader.LoadNextRoom();
        ObjectiveTracker.ClearObjectives();
        StartRoomTimer();
    }

    private void StartRoomTimer()
    {
        if (!CurrentLevel.IsTimeLimited)
            return;

        timer.StartTime(levelLoader.CurrentRoomData.BaseTimeLimit);
    }

    private void EndLevel()
    {
        LevelFinished?.Invoke(CurrentLevel);
        CurrentLevel = null;
        timer.StopTime();
    }

    private void EndRoom()
    {
        if (CurrentLevel == null)
            return;

        CompletedRoomCount++;
        RoomFinished?.Invoke();
        if (CompletedRoomCount < CurrentLevel.RoomCountToComplete)
            StartNextRoom();
        else
            EndLevel();
    }

    private void OnTimeOut()
    {
        EndRoom();
    }

    private void OnAllObjectivesInRoomCompleted()
    {
        EndRoom();
    }
}
