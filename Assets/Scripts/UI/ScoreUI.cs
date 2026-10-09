using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roomText;
    [SerializeField] StarContainer starContainer;
    [SerializeField] Transform ui;
    [SerializeField] LevelManager levelManager;
    [SerializeField] ScoreTracker scoreTracker;

    private void Update()
    {
        ui.gameObject.SetActive(levelManager.IsLevelRunning);
        if (!levelManager.IsLevelRunning)
            return;

        roomText.text = (scoreTracker.NumberOfCompletedRooms + 1) + "/" + scoreTracker.TotalNumberOfRooms;
        starContainer.gameObject.SetActive(levelManager.IsCurrentRoomTimed);
        if (levelManager.IsCurrentRoomTimed)
            starContainer.SetProgress(
                levelManager.GetStarFill(1),
                levelManager.GetStarFill(2),
                levelManager.GetStarFill(3));
    }
}