using UnityEngine;

public class ScoreTracker : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    public int TotalNumberOfRooms => levelManager.CurrentLevel == null
        ? 0 : levelManager.CurrentLevel.RoomCountToComplete;
    public int NumberOfCompletedRooms => levelManager.CompletedRoomCount;
    public int AchievableStars => levelManager.AchievableStars;
    public int FinalStars => levelManager.FinalStars;
}