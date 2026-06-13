using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTimer : MonoBehaviour
{
    public event Action TimeOut;

    public const float STARTING_MULTIPLIER = 1f;
    public const float FINAL_MULTIPLIER = 1.5f;

    [SerializeField] Timer timer;

    void Start()
    {
        timer.TimeOut += OnTimeOut;
    }

    public void StartRoomTime(int roomIndex, int totalRooms, float baseTimeLimit)
    {
        var progress = (float)roomIndex / totalRooms;
        var currentMultiplier = Mathf.Lerp(
            STARTING_MULTIPLIER, 
            FINAL_MULTIPLIER, 
            progress);

        timer.StartTime(baseTimeLimit / currentMultiplier);
    }

    public void StopTime()
    {
        timer.StopTime();
    }

    private void OnTimeOut()
    {
        TimeOut?.Invoke();
    }
}
