using System;
using UnityEngine;

public static class SaveDataManager
{
    public static event Action ScoresChanged;
    public static void AddStars(LevelData level, int stars)
    {
        stars = Mathf.Clamp(stars, 0, 3);
        if (stars > GetStars(level))
        {
            PlayerPrefs.SetInt(level.name + "_bestStars", stars);
            ScoresChanged?.Invoke();
        }
    }
    public static int GetStars(LevelData level)
    {
        var key = level.name + "_bestStars";
        if (PlayerPrefs.HasKey(key))
            return Mathf.Clamp(PlayerPrefs.GetInt(key), 0, 3);
        // Preserve stars earned by the previous objective-based score system.
        var legacyScore = PlayerPrefs.GetFloat(level.name + "_stars");
        return legacyScore >= 0.9f ? 3 : legacyScore >= 0.7f ? 2 : legacyScore >= 0.5f ? 1 : 0;
    }
    public static void DeleteAllSaveData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        ScoresChanged?.Invoke();
    }
}
