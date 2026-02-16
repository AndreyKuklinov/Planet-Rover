using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public int TargetScore { get; private set; }

    [SerializeField] string[] levelNames; 
    [SerializeField] float gameDuration;
    [SerializeField] float timeBoostMultiplier;
    [SerializeField] bool isTimeRunning = false;
    [SerializeField] bool isTestingMode;
    [SerializeField] string startingLevel;

    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }
    public float SecondsLeft { get; private set; }

    private Queue<string> levelQueue = new Queue<string>();
    private string currentLevel;

    void Start()
    {
        SecondsLeft = gameDuration;
        Rocket.SampleDelivered += OnSampleDelivered;
        Level.LevelCompleted += OnLevelCompleted;
        LoadLevel(startingLevel);
    }

    void Update()
    {
        TickDown();
    }

    public void StartGame()
    {
        if(!isTestingMode)
            isTimeRunning = true;

        StartNextLevel();
    }

    void StartNextLevel()
    {
        if (levelQueue.Count == 0)
            CreateLevelQueue();

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
        if (!isTimeRunning)
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
        StartNextLevel();
    }

    private void OnSampleDelivered(SampleData data)
    {
        Score += data.Value;
        SecondsLeft += timeBoostMultiplier * data.Value;
    }

    private void CreateLevelQueue()
    {
        levelQueue = new Queue<string>(levelNames);
    }
}
