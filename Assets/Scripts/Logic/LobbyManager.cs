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
        levelManager.StartLevel(level);
    }

    private void OnLevelCompleted(LevelData level)
    {
        if (IsRoomTesting || level.IsRoomTest) return;
        roomLoader.LoadRoom(lobbyPrefab);
    }
}
