using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public int TargetStars { get; private set; } = 3;
    [field: SerializeField] public bool IsTimeRunning { get; private set; } = false;

    [SerializeField] string[] levelNames;
    [SerializeField] int[] starThresholds;
    [SerializeField] float gameDuration;
    [SerializeField] float timeBoostMultiplier;
    [SerializeField] bool shouldTimeStartRunning = true;

    public bool IsGameOver { get; private set; }
    public bool IsGameWon { get; private set; }
    public int Score { get; private set; }
    public int Stars { get; private set; }
    public float SecondsLeft { get; private set; }

    private Queue<string> levelQueue = new Queue<string>();

    void Start()
    {
        SecondsLeft = gameDuration;
        Rocket.SampleDelivered += OnSampleDelivered;
        Level.LevelCompleted += OnLevelCompleted;
    }

    void Update()
    {
        TickDown();
    }

    void StartNextLevel()
    {
        if (levelQueue.Count == 0)
            CreateLevelQueue();

        var nextLevel = levelQueue.Count > 0 ? levelQueue.Dequeue() : SceneManager.GetActiveScene().name;
        LoadLevel(nextLevel);
    }

    void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }

    void TickDown()
    {
        if (!IsTimeRunning)
            return;

        SecondsLeft -= Time.deltaTime;
        if (SecondsLeft <= 0)
            LoseGame();
    }

    private void OnLevelCompleted()
    {
        if (shouldTimeStartRunning)
            IsTimeRunning = true;
        AwardStars();

        if(!IsGameOver && !IsGameWon)
            StartNextLevel();
    }

    private void OnSampleDelivered(SampleData data)
    {
        Score += data.Value;
        if(IsTimeRunning)
            SecondsLeft += timeBoostMultiplier * data.Value;
    }

    private void CreateLevelQueue()
    {
        levelQueue = new Queue<string>(levelNames);
    }

    private void LoseGame()
    {
        IsGameOver = true;
        Time.timeScale = 0f;
    }

    private void WinGame()
    {
        IsGameWon = true;
        IsTimeRunning = false;
    }

    private void AwardStars()
    {
        var stars = 0;
        foreach(var threshold in starThresholds)
        {
            if(Score >= threshold)
                stars++;
        }
        Stars = stars;

        if (Stars >= TargetStars)
            WinGame();
    }
}
