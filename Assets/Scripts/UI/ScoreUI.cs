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
    [SerializeField, Min(0f)] float starWarningDuration = 2f;
    [SerializeField, Min(0.01f)] float starBlinkInterval = 0.2f;
    TextMeshProUGUI stopwatchText;
    TextMeshProUGUI countdownText;

    private void Update()
    {
        bool showStopwatch = levelManager.IsTestingRoom;
        ui.gameObject.SetActive(levelManager.IsLevelRunning || levelManager.IsRoomTestFinished);
        if (!levelManager.IsLevelRunning && !levelManager.IsRoomTestFinished)
            return;

        roomText.gameObject.SetActive(!showStopwatch);
        if (stopwatchText != null) stopwatchText.gameObject.SetActive(showStopwatch);
        bool showCountdown = !showStopwatch && levelManager.IsCurrentRoomTimed;
        if (countdownText != null) countdownText.gameObject.SetActive(showCountdown);
        if (showStopwatch)
        {
            starContainer.gameObject.SetActive(false);
            if (stopwatchText == null)
            {
                stopwatchText = CreateTimerText("Room Test Stopwatch", false);
            }
            var elapsed = TimeSpan.FromSeconds(levelManager.ElapsedTime);
            stopwatchText.text = $"{(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
            return;
        }
        roomText.text = (scoreTracker.NumberOfCompletedRooms + 1) + "/" + scoreTracker.TotalNumberOfRooms;
        starContainer.gameObject.SetActive(levelManager.IsCurrentRoomTimed);
        if (levelManager.IsCurrentRoomTimed)
        {
            float untilLoss = levelManager.TimeUntilStarLoss;
            bool warning = starWarningDuration > 0f && untilLoss <= starWarningDuration;
            bool showLastStar = !warning || Mathf.Repeat(starWarningDuration - untilLoss,
                Mathf.Max(0.01f, starBlinkInterval) * 2f) < Mathf.Max(0.01f, starBlinkInterval);
            starContainer.SetStars(levelManager.AchievableStars, showLastStar);
            if (countdownText == null) countdownText = CreateTimerText("Level Countdown", false);
            int secondsRemaining = Mathf.CeilToInt(Mathf.Max(0f, levelManager.TimeRemaining));
            countdownText.text = $"{secondsRemaining / 60}:{secondsRemaining % 60:00}";
        }
    }

    TextMeshProUGUI CreateTimerText(string name, bool alignRight)
    {
        var text = Instantiate(roomText, ui);
        text.name = name;
        text.gameObject.SetActive(true);
        text.raycastTarget = false;
        text.enableWordWrapping = false;
        text.margin = Vector4.zero;
        text.alignment = alignRight ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
        var rect = text.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(alignRight ? 1f : 0f, 1f);
        rect.anchoredPosition = new Vector2(alignRight ? -10f : 10f, -10f);
        rect.sizeDelta = new Vector2(220f, 36f);
        return text;
    }
}
