using System;
using UnityEngine;

// Retains the existing scene component and timer reference, now timing a whole level.
public class RoomTimer : MonoBehaviour
{
    public event Action TimeOut;
    [SerializeField] Timer timer;
    public float ElapsedTime => timer.Duration - timer.TimeRemaining;
    public bool HasExpired => timer.Duration > 0f && timer.TimeRemaining <= 0f;

    private void OnEnable() => timer.TimeOut += OnTimeOut;
    private void OnDisable() => timer.TimeOut -= OnTimeOut;
    public void StartLevelTime(float duration)
    {
        if (duration > 0f)
            timer.StartTime(duration);
        else
            timer.StopTime();
        timer.SetPaused(true);
    }
    public void SetRoomTimed(bool isTimed) => timer.SetPaused(!isTimed);
    public void StopTime() => timer.StopTime();
    private void OnTimeOut() => TimeOut?.Invoke();
}