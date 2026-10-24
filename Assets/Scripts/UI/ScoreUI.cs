using System;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomText;
    [SerializeField] StarContainer starContainer;
    [SerializeField] Transform ui;
    [SerializeField] LevelManager levelManager;
    [SerializeField] ScoreTracker scoreTracker;
    TextMeshProUGUI stopwatchText;

    private void Update()
    {
        bool showStopwatch = levelManager.IsTestingRoom;
        ui.gameObject.SetActive(levelManager.IsLevelRunning || levelManager.IsRoomTestFinished);
        if (!levelManager.IsLevelRunning && !levelManager.IsRoomTestFinished)
            return;

        roomText.gameObject.SetActive(!showStopwatch);
        if (stopwatchText != null) stopwatchText.gameObject.SetActive(showStopwatch);
        if (showStopwatch)
        {
            starContainer.gameObject.SetActive(false);
            if (stopwatchText == null)
            {
                stopwatchText = Instantiate(roomText, ui);
                stopwatchText.name = "Room Test Stopwatch";
                stopwatchText.gameObject.SetActive(true);
                stopwatchText.raycastTarget = false;
                stopwatchText.enableWordWrapping = false;
                stopwatchText.margin = Vector4.zero;
                stopwatchText.alignment = TextAlignmentOptions.MidlineLeft;
                var rect = stopwatchText.rectTransform;
                rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f, -10f);
                rect.sizeDelta = new Vector2(220f, 36f);
            }
            var elapsed = TimeSpan.FromSeconds(levelManager.ElapsedTime);
            stopwatchText.text = $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
            return;
        }
        roomText.text = (scoreTracker.NumberOfCompletedRooms + 1) + "/" + scoreTracker.TotalNumberOfRooms;
        starContainer.gameObject.SetActive(levelManager.IsCurrentRoomTimed);
        if (levelManager.IsCurrentRoomTimed)
            starContainer.SetProgress(
                levelManager.GetStarFill(1),
                levelManager.GetStarFill(2),
                levelManager.GetStarFill(3));
    }
}
