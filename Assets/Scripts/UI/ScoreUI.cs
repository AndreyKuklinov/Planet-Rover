using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomText;
    [SerializeField] StarContainer starContainer;
    [SerializeField] Image powerImage;
    [SerializeField] Transform ui;
    [SerializeField] Transform powerTransform;
    [SerializeField] LevelManager levelManager;
    [SerializeField] ScoreTracker scoreTracker;
    [SerializeField] Timer roomTimer;
    [SerializeField] TextMeshProUGUI scoreText;

    bool IsVisible
        => levelManager.IsLevelRunning;

    bool IsPowerVisible
        => levelManager.CurrentLevel.IsTimeLimited;

    void Update()
    {
        ui.gameObject.SetActive(IsVisible);
        if (!IsVisible)
            return;

        roomText.text = GetRoomText();
        scoreText.text = GetScoreText();
        UpdateStars();
        UpdatePower();
    }

    string GetRoomText()
    {
        return (scoreTracker.NumberOfCompletedRooms+1) + "/" + scoreTracker.TotalNumberOfRooms;
    }

    string GetScoreText()
    {
        return ((int)(scoreTracker.CurrentScore * 100)).ToString() + "%";
    }

    void UpdateStars()
    {
        var score = scoreTracker.BestPossibleScore;
        var stars = StarCalculator.GetNumberOfStars(score);
        starContainer.SetStars(stars);
    }

    void UpdatePower()
    {
        powerTransform.gameObject.SetActive(IsPowerVisible);
        if (!IsPowerVisible)
            return;

        var value = roomTimer.TimeRemaining / roomTimer.Duration;
        powerImage.fillAmount = Mathf.Clamp(value, 0, 1);
    }
}
