using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public const int MAX_STARS = 3;

    public event Action LevelCompleted;

    public LevelData CurrentLevel {  get; private set; }
    public int Stars { get; private set; }
    public int CompletedRoomCount { get; private set; }

    [SerializeField] LevelLoader levelLoader;
    [SerializeField] Timer timer;

    void OnEnable()
    {
        Room.RoomCompleted += OnRoomCompleted;
        timer.TimeOut += OnTimeOut;
    }

    void OnDisable()
    {
        Room.RoomCompleted -= OnRoomCompleted;
    }

    public void StartLevel(LevelData levelData)
    {
        CurrentLevel = levelData;
        Stars = MAX_STARS;
        CompletedRoomCount = 0;
        levelLoader.SetLevelData(levelData);
        StartNextRoom();
    }

    private void StartNextRoom()
    {
        levelLoader.LoadNextRoom();
        //if (CurrentLevel.IsTimeLimited)
        //    timer.StartTime(CurrentRoom)
    }

    private void OnRoomCompleted()
    {
        CompletedRoomCount++;
        if (CompletedRoomCount < CurrentLevel.RoomCountToComplete)
            levelLoader.LoadNextRoom();
        else
        {
            LevelCompleted?.Invoke();
            CurrentLevel = null;
        }
    }

    private void OnTimeOut()
    {
        throw new NotImplementedException();
    }
}
