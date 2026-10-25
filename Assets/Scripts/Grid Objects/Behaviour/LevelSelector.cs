using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour, IInteractable
{
    [SerializeField] LevelData levelData;
    [SerializeField] StarContainer starContainer;
    [SerializeField] LevelDataEventChannel levelSelected;

    void OnEnable()
    {
        SaveDataManager.ScoresChanged += DisplayStars;
        DisplayStars();
    }

    void OnDisable() => SaveDataManager.ScoresChanged -= DisplayStars;

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
        var stars = SaveDataManager.GetStars(levelData);
        starContainer.SetStars(stars);
    }
}
