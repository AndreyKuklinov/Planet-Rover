using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class LogicController : MonoBehaviour
{
    [SerializeField] VoidEventChannel quitTriggered;
    [SerializeField] VoidEventChannel restartTriggered;
    [SerializeField] VoidEventChannel deleteSaveDataTriggered;
    [SerializeField] LevelManager levelManager;

    void Awake()
    {
        quitTriggered.Raised += OnQuitTriggered;
        restartTriggered.Raised += OnRestartTriggered;
        deleteSaveDataTriggered.Raised += OnDeleteTriggered;
    }

    private void OnDeleteTriggered()
    {
        Debug.Log("Deleted all save data");
        SaveDataManager.DeleteAllSaveData();
    }

    private void OnRestartTriggered()
    {
        if (!levelManager.IsLevelRunning)
            return;

        levelManager.RestartLevel();
        Debug.Log("Restarted level");
    }

    private void OnQuitTriggered()
    {
        if(levelManager.IsLevelRunning)
        {
            Debug.Log("Quit level");
            levelManager.StopCurrentLevel();
            return;
        }

        Debug.Log("Quitting game");
        Application.Quit();
    }
}
