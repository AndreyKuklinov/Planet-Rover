using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public static class SaveDataManager
{
    public static void AddScore(LevelData levelData, float score)
    {
        var prevScore = GetScore(levelData);
        if (prevScore >= score)
            return;
        PlayerPrefs.SetFloat(GetLevelScoreString(levelData), score);
    }

    public static float GetScore(LevelData levelData)
    {
        return PlayerPrefs.GetFloat(GetLevelScoreString(levelData));
    }

    private static string GetLevelScoreString(LevelData levelData)
    {
        return levelData.name + "_stars";
    }
}
