using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] float gameDuration;
    [SerializeField] float timeBoostPerLevel;
    [SerializeField] bool isTimeRunning;

    public bool IsGameOver { get; private set; }

    float secondsUntilGameover;

    void Start()
    {
        secondsUntilGameover = gameDuration;
    }

    void Update()
    {
        TickDown();
    }

    void CompleteLevel()
    {
        secondsUntilGameover += timeBoostPerLevel;
    }

    void TickDown()
    {
        if (!isTimeRunning)
            return;

        secondsUntilGameover -= Time.deltaTime;
        if (secondsUntilGameover <= 0)
        {
            IsGameOver = true;
            Time.timeScale = 0f;
        }
    }
}
