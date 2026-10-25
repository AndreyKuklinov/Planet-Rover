using System;
using UnityEngine;

// Retains the existing scene component and timer reference, now timing a whole level.
public class RoomTimer : MonoBehaviour
{
    public event Action TimeOut;
    [SerializeField] Timer timer;
    bool isStopwatch;
    bool isStopwatchRunning;
    float stopwatchElapsed;
    public float ElapsedTime => isStopwatch ? stopwatchElapsed : timer.Duration - timer.TimeRemaining;
    public float TimeRemaining => timer.TimeRemaining;
    public bool HasExpired => !isStopwatch && timer.Duration > 0f && timer.TimeRemaining <= 0f;

    private void OnEnable() => timer.TimeOut += OnTimeOut;
    private void OnDisable() => timer.TimeOut -= OnTimeOut;
    public void StartLevelTime(float duration)
    {
        isStopwatch = isStopwatchRunning = false;
        if (duration > 0f)
            timer.StartTime(duration);
        else
            timer.StopTime();
        timer.SetPaused(true);
    }
    public void StartStopwatch()
    {
        timer.StopTime();
        stopwatchElapsed = 0f;
        isStopwatch = isStopwatchRunning = true;
    }
    void Update()
    {
        if (isStopwatchRunning) stopwatchElapsed += Time.deltaTime;
    }
    public void SetRoomTimed(bool isTimed)
    {
        if (!isStopwatch) timer.SetPaused(!isTimed);
    }
    public void StopTime()
    {
        isStopwatchRunning = false;
        timer.StopTime();
    }
    private void OnTimeOut() => TimeOut?.Invoke();
}
