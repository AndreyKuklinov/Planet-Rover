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
    int lastDeleteFrame = -1;

    void OnEnable()
    {
        quitTriggered.Raised += OnQuitTriggered;
        restartTriggered.Raised += OnRestartTriggered;
        deleteSaveDataTriggered.Raised += OnDeleteTriggered;
    }

    void OnDisable()
    {
        quitTriggered.Raised -= OnQuitTriggered;
        restartTriggered.Raised -= OnRestartTriggered;
        deleteSaveDataTriggered.Raised -= OnDeleteTriggered;
    }

    void Update()
    {
        // Delete is a global shortcut, even before a keyboard player has joined.
        foreach (var device in InputSystem.devices)
            if (device is Keyboard keyboard && keyboard.deleteKey.wasPressedThisFrame)
            {
                OnDeleteTriggered();
                break;
            }
    }

    private void OnDeleteTriggered()
    {
        // A paired keyboard's action and the global shortcut can arrive in the same frame.
        if (lastDeleteFrame == Time.frameCount) return;
        lastDeleteFrame = Time.frameCount;
        Debug.Log("Deleted all save data");
        SaveDataManager.DeleteAllSaveData();
    }

    private void OnRestartTriggered()
    {
        if (!levelManager.IsLevelRunning && !levelManager.IsRoomTestFinished)
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
