using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
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
        UpdateScore();
        LevelCompleted?.Invoke();
        CurrentLevel = null;
        timer.StopTime();
    }

    private void UpdateScore()
    {
        var s = CurrentLevel.PrefsString;
        var score = PlayerPrefs.GetInt(s);

        if (Stars > score)
            PlayerPrefs.SetInt(s, Stars);
    }

    private void OnRoomCompleted()
    {
        CompletedRoomCount++;
        if (CompletedRoomCount < CurrentLevel.RoomCountToComplete)
            StartNextRoom();
        else
            EndLevel();
    }

    private void OnTimeOut()
    {
        Stars--;
        Debug.Log(Stars);
        if(Stars > 0)
        {
            StartRoomTimer();
            return;
        }

        EndLevel();
    }
}
