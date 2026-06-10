using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour, IInteractable
{
    [SerializeField] LevelData levelData;
    [SerializeField] StarContainer starContainer;
    [SerializeField] LevelDataEventChannel levelSelected;

    void Start()
    {
        //starContainer.SetStars(SaveDataManager.GetScore(levelData));
        
        // TESTING
        //Debug.Log(levelData.name + ": " + SaveDataManager.GetScore(levelData));
    }

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return true;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        levelSelected.Raise(levelData);
        return grabbedObject;
    }
}
