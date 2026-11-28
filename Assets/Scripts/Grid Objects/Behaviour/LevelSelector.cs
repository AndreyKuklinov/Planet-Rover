using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelSelector : MonoBehaviour, IInteractable
{
    [SerializeField] LevelData levelData;
    [SerializeField] StarContainer starContainer;
    [SerializeField] LevelDataEventChannel levelSelected;
    [SerializeField] GameObject lockIcon;
    [SerializeField] TMP_Text starRequirement;
    [SerializeField] RectTransform titleRect;

    [Header("Unlock Requirements")]
    [SerializeField, Tooltip("Earn at least one star on this level to unlock this selector.")]
    LevelData requiredLevel;
    [SerializeField, Range(0, 100), Tooltip("Percentage of all possible stars on this planet's distinct Standard levels. Zero disables this requirement.")]
    int requiredPlanetStarPercentage;

    bool unlocked;

    void OnEnable()
    {
        SaveDataManager.ScoresChanged += Refresh;
        Refresh();
    }

    void OnDisable() => SaveDataManager.ScoresChanged -= Refresh;

    public bool CanInteractWith(IGrabbable grabbedObject) => unlocked;

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        if (unlocked) levelSelected.Raise(levelData);
        return grabbedObject;
    }

    void Refresh()
    {
        unlocked = levelData != null && (requiredLevel == null || SaveDataManager.GetStars(requiredLevel) > 0);
        bool showStarCount = false;
        if (unlocked && requiredPlanetStarPercentage > 0)
        {
            var (earned, required) = GetPlanetStarRequirement();
            unlocked = required > 0 && earned >= required;
            showStarCount = required > 0 && earned < required;
            if (showStarCount && starRequirement != null)
                starRequirement.text = $"{earned}/{required}";
        }

        if (lockIcon != null) lockIcon.SetActive(!unlocked);
        if (starRequirement != null) starRequirement.gameObject.SetActive(showStarCount);
        if (titleRect != null)
            titleRect.anchorMin = new Vector2(titleRect.anchorMin.x, showStarCount ? 0.4f : unlocked ? 0f : 0.2f);
        int stars = unlocked && levelData.Mode != LevelMode.Challenge ? SaveDataManager.GetStars(levelData) : 0;
        starContainer.SetStars(stars);
    }

    (int earned, long required) GetPlanetStarRequirement()
    {
        var planet = GetComponentInParent<Room>();
        if (planet == null) return (0, 0);

        var levels = new HashSet<LevelData>();
        int earned = 0;
        foreach (var selector in planet.GetComponentsInChildren<LevelSelector>(true))
        {
            var level = selector.levelData;
            if (level == null || level.Mode != LevelMode.Standard || level.IsWorkshop || !levels.Add(level))
                continue;
            earned += SaveDataManager.GetStars(level);
        }
        long required = (requiredPlanetStarPercentage * 3L * levels.Count + 99) / 100;
        return (earned, required);
    }
}
