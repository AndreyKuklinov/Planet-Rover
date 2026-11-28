using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public bool IsRoomTesting { get; set; }
    [SerializeField] Room lobbyPrefab;
    [SerializeField] LevelManager levelManager;
    [SerializeField] RoomLoader roomLoader;
    [SerializeField] LevelDataEventChannel levelSelected;

    void Start()
    {
        roomLoader.LoadRoom(lobbyPrefab);
    }

    void OnEnable()
    {
        levelSelected.Raised += OnLevelSelected;
        levelManager.LevelFinished += OnLevelCompleted;
    }

    void OnDisable()
    {
        levelSelected.Raised -= OnLevelSelected;
        levelManager.LevelFinished -= OnLevelCompleted;
    }

    private void OnLevelSelected(LevelData level)
    {
        if (IsRoomTesting) return;
        if (level.IsWorkshop)
        {
            OpenWorkshop(level.Rooms[0], null, false);
            return;
        }
        levelManager.StartLevel(level);
    }

    public void OpenWorkshop(Room template, RoomWorkshop.Draft draft, bool play)
    {
        levelManager.ObjectiveTracker.ClearObjectives();
        roomLoader.LoadRoom(template);
        roomLoader.CurrentRoom.GetComponent<RoomWorkshop>().Open(this, template, draft, play);
    }

    public void LeaveWorkshop()
    {
        levelManager.ObjectiveTracker.ClearObjectives();
        roomLoader.LoadRoom(lobbyPrefab);
    }

    private void OnLevelCompleted(LevelData level)
    {
        if (IsRoomTesting || level.IsRoomTest) return;
        roomLoader.LoadRoom(lobbyPrefab);
    }
}
