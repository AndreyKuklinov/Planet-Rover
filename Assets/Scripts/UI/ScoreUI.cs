using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] StarContainer starContainer;
    [SerializeField] Image powerImage;
    [SerializeField] Transform ui;
    [SerializeField] LevelManager levelManager;
    [SerializeField] ScoreTracker scoreTracker;
    [SerializeField] Timer roomTimer;

    bool IsVisible
        => levelManager.IsLevelRunning;

    void Update()
    {
        ui.gameObject.SetActive(IsVisible);
        if (!IsVisible)
            return;

        textMesh.text = GetScoreText();
        UpdateStars();
        UpdatePower();
    }

    string GetScoreText()
    {
        return (scoreTracker.NumberOfCompletedRooms+1) + "/" + scoreTracker.TotalNumberOfRooms;
    }

    void UpdateStars()
    {
        var score = scoreTracker.BestPossibleScore;
        var stars = StarCalculator.GetNumberOfStars(score);
        starContainer.SetStars(stars);
    }

    void UpdatePower()
    {
        var value = roomTimer.TimeRemaining / roomTimer.Duration;
        powerImage.fillAmount = Mathf.Clamp(value, 0, 1);
    }
}
