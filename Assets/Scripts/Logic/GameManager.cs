using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static event Action GameEnded;

    GameState gameState = GameState.Lobby;
    public LevelSet LevelSet { get; private set; }
    public int Score { get; private set; }
    public int Stars { get; private set; }
    public float SecondsLeft { get; private set; }

    public bool IsGameOver
        => gameState == GameState.Won || gameState == GameState.Lost;

    public bool IsTimeRunning
        => gameState == GameState.Running && LevelSet != null && LevelSet.IsTimeLimited;

    private Queue<string> levelQueue = new Queue<string>();
    private Level currentLevel;
    private string lobbySceneName;

    public void StartGame(LevelSet levelSet)
    {
        LevelSet = levelSet;
        gameState = GameState.Running;
        if(levelSet.IsTimeLimited)
            SecondsLeft = levelSet.GameDuration;
        // TODO: This is a nightmare, fix later
        lobbySceneName = SceneManager.GetActiveScene().name;
        StartNextLevel();
    }

    public void GoToLobby()
    {
        if (lobbySceneName == null)
            throw new InvalidOperationException("Lobby scene name not found");

        ResetGame();
        LoadLevel(lobbySceneName);
    }

    void Start()
    {
        Rocket.SampleDelivered += OnSampleDelivered;
        Level.LevelCompleted += OnLevelCompleted;
        Level.LevelStarted += OnLevelStarted;
        LevelSelector.LevelSetSelected += OneLevelSetSelected;
    }

    private void OnDestroy()
    {
        Rocket.SampleDelivered -= OnSampleDelivered;
        Level.LevelCompleted -= OnLevelCompleted;
        Level.LevelStarted -= OnLevelStarted;
        LevelSelector.LevelSetSelected -= OneLevelSetSelected;
    }

    void Update()
    {
        TickDown();
    }

    void StartNextLevel()
    {
        if (levelQueue.Count == 0)
            levelQueue = CreateLevelQueue();

        var nextLevel = levelQueue.Count > 0
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
            EndGame(Stars >= LevelSet.StarThresholds.Length);  
    }

    private void OnLevelStarted(Level obj)
    {
        currentLevel = obj;
    }

    private void OnLevelCompleted()
    {
        if(gameState == GameState.Running)
            StartNextLevel();
    }

    private void OneLevelSetSelected(LevelSet obj)
    {
        StartGame(obj);
    }

    private void OnSampleDelivered(SampleData data)
    {
        if (gameState != GameState.Running)
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
        gameState = victory ? GameState.Won : GameState.Lost;
        GameEnded?.Invoke();
    }

    private void ResetGame()
    {
        gameState = GameState.Lobby;
        Score = 0;
        Stars = 0;
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
        UpdateHighScore();

        if (LevelSet.GoToLobbyWhenWon && Stars >= LevelSet.StarThresholds.Length)
        {
            EndGame(true);
            GoToLobby();
        }
    }

    private void UpdateHighScore()
    {
        var s = LevelSet.PrefsString;
        var highScore = PlayerPrefs.GetInt(s);

        if(Stars > highScore)
            PlayerPrefs.SetInt(s, Stars);
    }
}
