using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] bool isLoopEnabled = false;

    public GameState GameState { get; private set; } = GameState.None;
    public LevelSet LevelSet { get; private set; }
    public int Score { get; private set; }
    public int Stars { get; private set; }
    public float SecondsLeft { get; private set; }

    private Queue<string> levelQueue = new Queue<string>();
    private Level currentLevel;

    public void StartGame(LevelSet levelSet)
    {
        this.LevelSet = levelSet;
        GameState = GameState.Running;
        if(levelSet.IsTimeLimited)
            SecondsLeft = levelSet.GameDuration;
        StartNextLevel();
    }

    void Start()
    {
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
            levelQueue = CreateLevelQueue();

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
        if (GameState != GameState.Running || !LevelSet.IsTimeLimited)
            return;

        SecondsLeft -= Time.deltaTime;
        if (SecondsLeft <= 0)
            EndGame(Stars >= LevelSet.StarThresholds.Length);  
    }

    private void OnLevelStarted(Level obj)
    {
        currentLevel = obj;
    }

    private void OnLevelCompleted()
    {
        if(GameState == GameState.Running)
            StartNextLevel();
    }

    private void OnSampleDelivered(SampleData data)
    {
        if (GameState != GameState.Running)
            return;

        if (currentLevel == null)
            throw new InvalidOperationException("Trying to deliver a sample, while currentLevel is null");

        Score += data.Value;
        SecondsLeft += currentLevel.TimeBoostMultiplier * data.Value;
        AwardStars();
    }

    private Queue<string> CreateLevelQueue()
    {
        var levels = LevelSet.IsOrderRandom 
            ? LevelSet.LevelNames.OrderBy(x => UnityEngine.Random.value).ToArray()
            : LevelSet.LevelNames;
        
        return new Queue<string>(levels);
    }

    private void EndGame(bool victory)
    {
        GameState = victory ? GameState.Won : GameState.Lost;

        // TESTING
        Time.timeScale = 0;
    }

    private void AwardStars()
    {
        var stars = 0;
        foreach(var threshold in LevelSet.StarThresholds)
        {
            if(Score >= threshold)
                stars++;
        }
        Stars = stars;

        if (LevelSet.EndGameWhenWon && Stars > LevelSet.StarThresholds.Length)
            EndGame(true);
    }
}
