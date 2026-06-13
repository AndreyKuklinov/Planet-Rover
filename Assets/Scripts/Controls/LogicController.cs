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

    void Awake()
    {
        quitTriggered.Raised += OnQuitTriggered;
        restartTriggered.Raised += OnRestartTriggered;
        deleteSaveDataTriggered.Raised += OnDeleteTriggered;
    }

    private void OnDeleteTriggered()
    {
        Debug.Log("Save data deleted");
    }

    private void OnRestartTriggered()
    {
        Debug.Log("Level restarted");
    }

    private void OnQuitTriggered()
    {
        Debug.Log("Quitting");
    }
}
