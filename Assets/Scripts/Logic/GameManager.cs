using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public int TargetScore { get; private set; }
    [field: SerializeField] public bool IsTimeRunning { get; private set; } = false;

    [SerializeField] string[] levelNames; 
    [SerializeField] float gameDuration;
    [SerializeField] float timeBoostMultiplier;
    [SerializeField] bool shouldTimeStartRunning = true;

    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }
    public float SecondsLeft { get; private set; }

    private Queue<string> levelQueue = new Queue<string>();
    private string currentLevel = null;

    void Start()
    {
        SecondsLeft = gameDuration;
        Rocket.SampleDelivered += OnSampleDelivered;
        Level.LevelCompleted += OnLevelCompleted;
        StartNextLevel();
    }

    void Update()
    {
        TickDown();
    }

    void StartNextLevel()
    {
        if (levelQueue.Count == 0)
            CreateLevelQueue();

        if(currentLevel != null)
            SceneManager.UnloadSceneAsync(currentLevel);

        var nextLevel = levelQueue.Count > 0 ? levelQueue.Dequeue() : currentLevel;
        LoadLevel(nextLevel);
    }

    void LoadLevel(string levelName)
    {
        SceneManager.LoadScene(levelName, LoadSceneMode.Additive);
        currentLevel = levelName;
    }

    void TickDown()
    {
        if (!IsTimeRunning)
            return;

        SecondsLeft -= Time.deltaTime;
        if (SecondsLeft <= 0)
        {
            IsGameOver = true;
            Time.timeScale = 0f;
        }
    }

    private void OnLevelCompleted()
    {
        if (shouldTimeStartRunning)
            IsTimeRunning = true;
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
}
