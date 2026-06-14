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
    [SerializeField] RoomLoader roomLoader;
    [field: SerializeField] public ObjectiveTracker ObjectiveTracker { get; private set; }
    [SerializeField] RoomTimer roomTimer;
    [SerializeField] VoidEventChannel roomExitTriggered;

    public bool IsLevelRunning
        => CurrentLevel != null;

    void OnEnable()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled += OnAllObjectivesInRoomCompleted;
        roomExitTriggered.Raised += OnRoomExitTriggered;
        roomTimer.TimeOut += OnTimeOut;
        roomLoader.LoadedRoom += OnRoomLoaded;
    }

    void OnDisable()
    {
        ObjectiveTracker.AllObjectivesWereFulfilled -= OnAllObjectivesInRoomCompleted;
        roomTimer.TimeOut -= OnTimeOut;
        roomLoader.LoadedRoom -= OnRoomLoaded;
        roomExitTriggered.Raised -= OnRoomExitTriggered;
    }

    public void StartLevel(LevelData levelData)
    {
        CurrentLevel = levelData;
        CompletedRoomCount = 0;
        levelLoader.SetLevelData(levelData);
        LevelStarted?.Invoke();
        StartNextRoom();
    }

    public void StopCurrentLevel()
    {
        if (!IsLevelRunning)
            throw new InvalidOperationException("Trying to stop the level, when no level is active");

        EndLevel();
    }

    public void RestartLevel()
    {
        StartLevel(CurrentLevel);
    }

    private void StartNextRoom()
    {
        //Debug.Log("Started next room");
        levelLoader.LoadNextRoom();
        StartRoomTimer();
    }

    private void StartRoomTimer()
    {
        if (!CurrentLevel.IsTimeLimited)
            return;

        roomTimer.StartRoomTime(
            CompletedRoomCount, 
            CurrentLevel.RoomCountToComplete, 
            levelLoader.CurrentRoomData.BaseTimeLimit
        );
    }

    private void EndLevel()
    {
        LevelFinished?.Invoke(CurrentLevel);
        CurrentLevel = null;
        roomTimer.StopTime();
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
        //EndRoom();
    }
    private void OnRoomExitTriggered()
    {
        EndRoom();
    }

    private void OnRoomLoaded()
    {
        ObjectiveTracker.ClearObjectives();
    }
}
