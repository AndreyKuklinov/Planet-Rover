using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] Room lobbyPrefab;
    [SerializeField] LevelManager levelManager;
    [SerializeField] RoomLoader roomLoader;

    void Start()
    {
        roomLoader.LoadRoom(lobbyPrefab);
    }

    void OnEnable()
    {
        LevelSelector.LevelSetSelected += OnLevelSelected;
        levelManager.LevelCompleted += OnLevelCompleted;
    }

    void OnDisable()
    {
        LevelSelector.LevelSetSelected -= OnLevelSelected;
        levelManager.LevelCompleted -= OnLevelCompleted;
    }

    private void OnLevelSelected(LevelData level)
    {
        levelManager.StartLevel(level);
    }

    private void OnLevelCompleted()
    {
        roomLoader.LoadRoom(lobbyPrefab);
    }
}
