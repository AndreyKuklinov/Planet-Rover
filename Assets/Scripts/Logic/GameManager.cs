using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class GameManager : MonoBehaviour
{
    public static event Action GameEnded;

    [SerializeField] Room lobbyLevelPrefab;

    GameState gameState = GameState.Lobby;
    public LevelSet LevelSet { get; private set; }
    public int Score { get; private set; }
    public int Stars { get; private set; }
    public float SecondsLeft { get; private set; }

    public bool IsGameOver
        => gameState == GameState.Won || gameState == GameState.Lost;

    public bool IsTimeRunning
        => gameState == GameState.Running && LevelSet != null && LevelSet.IsTimeLimited;

    private Queue<Room> levelQueue;
    private Room currentLevel;

    public void StartGame(LevelSet levelSet)
    {
        LevelSet = levelSet;
        gameState = GameState.Running;
        if(levelSet.IsTimeLimited)
            SecondsLeft = levelSet.GameDuration;
        levelQueue = new Queue<Room>();
        Destroy(currentLevel.gameObject);
        StartNextLevel();
    }

    public void LoadLobby()
    {
        if (lobbyLevelPrefab == null)
            throw new InvalidOperationException("Lobby level not found");

        ResetGame();
        LoadLevel(lobbyLevelPrefab);
    }

    void Start()
    {
        Rocket.SampleDelivered += OnSampleDelivered;
        Room.LevelCompleted += OnLevelCompleted;
        Room.LevelStarted += OnLevelStarted;
        LevelSelector.LevelSetSelected += OneLevelSetSelected;
        LoadLobby();
    }

    private void OnDestroy()
    {
        Rocket.SampleDelivered -= OnSampleDelivered;
        Room.LevelCompleted -= OnLevelCompleted;
        Room.LevelStarted -= OnLevelStarted;
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
            : currentLevel;
        LoadLevel(nextLevel);
    }

    void LoadLevel(Room level)
    {
        if(currentLevel != null)
            Destroy(currentLevel.gameObject);

        var newLevel = Instantiate(level, transform);
        currentLevel = newLevel;
    }

    void TickDown()
    {
        if (!IsTimeRunning)
            return;

        SecondsLeft -= Time.deltaTime;
        if (SecondsLeft <= 0)
            EndGame(Stars >= LevelSet.StarThresholds.Length);  
    }

    private void OnLevelStarted(Room obj)
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

    private Queue<Room> CreateLevelQueue()
    {
        var levels = LevelSet.IsOrderRandom 
            ? LevelSet.Levels.OrderBy(x => UnityEngine.Random.value).ToArray()
            : LevelSet.Levels;
        
        return new Queue<Room>(levels);
    }

    private void EndGame(bool victory)
    {
        gameState = victory ? GameState.Won : GameState.Lost;
        Time.timeScale = 0;
        GameEnded?.Invoke();
    }

    private void ResetGame()
    {
        Time.timeScale = 1;
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
            LoadLobby();
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
