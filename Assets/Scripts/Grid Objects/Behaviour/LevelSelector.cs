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
        DisplayStars();
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

    private void DisplayStars()
    {
        var score = SaveDataManager.GetScore(levelData);
        var stars = StarCalculator.GetNumberOfStars(score);
        starContainer.SetStars(stars);
    }
}
