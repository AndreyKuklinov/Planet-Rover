using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public event Action LevelCompleted;

    public LevelData CurrentLevel {  get; private set; }
    public int CompletedRoomCount { get; private set; }

    [SerializeField] LevelLoader levelLoader;
    [SerializeField] Timer timer;

    void OnEnable()
    {
        Room.RoomCompleted += EndRoom;
        timer.TimeOut += OnTimeOut;
    }

    void OnDisable()
    {
        Room.RoomCompleted -= EndRoom;
    }

    public void StartLevel(LevelData levelData)
    {
        CurrentLevel = levelData;
        CompletedRoomCount = 0;
        levelLoader.SetLevelData(levelData);
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
        LevelCompleted?.Invoke();
        CurrentLevel = null;
        timer.StopTime();
    }

    private void EndRoom()
    {
        CompletedRoomCount++;
        if (CompletedRoomCount < CurrentLevel.RoomCountToComplete)
            StartNextRoom();
        else
            EndLevel();
    }

    private void OnTimeOut()
    {
        EndRoom();
    }
}
