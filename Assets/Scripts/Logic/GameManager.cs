using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public int TargetStars { get; private set; } = 3;
    [field: SerializeField] public bool IsTimeRunning { get; private set; } = false;

    [SerializeField] string[] levelNames;
    [SerializeField] int[] starThresholds;
    [field: SerializeField] public float GameDuration { get; private set; } = 30;
    [SerializeField] bool shouldTimeStartRunning = true;
    [SerializeField] bool isLoopEnabled = false;

    public bool IsGameOver { get; private set; }
    public bool IsGameWon { get; private set; }
    public int Score { get; private set; }
    public int Stars { get; private set; }
    public float SecondsLeft { get; private set; }

    private Queue<string> levelQueue = new Queue<string>();
    private Level currentLevel;

    void Start()
    {
        SecondsLeft = GameDuration;
        Rocket.SampleDelivered += OnSampleDelivered;
        Level.LevelCompleted += OnLevelCompleted;
        Level.LevelStarted += OnLevelStarted;
    }

    private void OnDestroy()
    {
        Rocket.SampleDelivered -= OnSampleDelivered;
        Level.LevelCompleted -= OnLevelCompleted;
        Level.LevelStarted -= OnLevelStarted;
    }

    void Update()
    {
        TickDown();
    }

    void StartNextLevel()
    {
        if (levelQueue.Count == 0)
            CreateLevelQueue();

        var nextLevel = levelQueue.Count > 0 && !isLoopEnabled 
            ? levelQueue.Dequeue() 
            : SceneManager.GetActiveScene().name;
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
        {
            if (Stars >= TargetStars)
                WinGame();
            else
                LoseGame();
        }
            
    }

    private void OnLevelStarted(Level obj)
    {
        currentLevel = obj;
    }

    private void OnLevelCompleted()
    {
        if (shouldTimeStartRunning && !IsGameOver && !IsGameWon)
            IsTimeRunning = true;
        AwardStars();

        if(!IsGameOver && !IsGameWon)
            StartNextLevel();

        if(IsGameWon)
            IsTimeRunning = false;
    }

    private void OnSampleDelivered(SampleData data)
    {
        if (!IsTimeRunning)
            return;

        if (currentLevel == null)
            throw new InvalidOperationException("Trying to deliver a sample, while the level hasn't loaded");

        Score += data.Value;
        SecondsLeft += currentLevel.TimeBoostMultiplier * data.Value;
        AwardStars();
    }

    private void CreateLevelQueue()
    {
        levelQueue = new Queue<string>(levelNames.OrderBy(x => UnityEngine.Random.value));
    }

    private void LoseGame()
    {
        IsGameOver = true;
        IsTimeRunning = false;
        Time.timeScale = 0;
    }

    private void WinGame()
    {
        IsGameWon = true;
        IsTimeRunning = false;
        Time.timeScale = 0;
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
    }
}
