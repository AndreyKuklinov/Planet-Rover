using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public int TargetScore { get; private set; }

    [SerializeField] float gameDuration;
    [SerializeField] float timeBoostMultiplier;
    [SerializeField] bool isTimeRunning;

    public bool IsGameOver { get; private set; }
    public int Score { get; private set; }
    public float SecondsLeft { get; private set; }

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
}
