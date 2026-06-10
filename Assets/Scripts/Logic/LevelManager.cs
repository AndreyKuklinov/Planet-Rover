using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public event Action<LevelData> LevelFinished;
    public event Action LevelStarted;
    public event Action<Room> RoomFinished;

    public LevelData CurrentLevel {  get; private set; }
    public Room CurrentRoom
        => levelLoader.CurrentRoom;
    public int CompletedRoomCount { get; private set; }

    [SerializeField] LevelLoader levelLoader;
    [SerializeField] Timer timer;

    void OnEnable()
    {
        Room.AllObjectivesCompleted += OnAllObjectivesInRoomCompleted;
        timer.TimeOut += OnTimeOut;
    }

    void OnDisable()
    {
        Room.AllObjectivesCompleted -= OnAllObjectivesInRoomCompleted;
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
        RoomFinished?.Invoke(CurrentRoom);
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
