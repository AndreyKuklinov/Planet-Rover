using UnityEngine;

[CreateAssetMenu(menuName = "Levels/Star Config")]
public class StarConfig : ScriptableObject
{
    [SerializeField, Min(0.01f)] private float threeStarMultiplier = 1f;
    [SerializeField, Min(0.01f)] private float twoStarMultiplier = 2f;
    [SerializeField, Min(0.01f)] private float oneStarMultiplier = 3f;

    public float MaximumMultiplier => oneStarMultiplier;
    public bool IsValid => threeStarMultiplier > 0f
        && twoStarMultiplier >= threeStarMultiplier
        && oneStarMultiplier > twoStarMultiplier
        && !float.IsInfinity(oneStarMultiplier);

    public int GetStars(float elapsedTime, float standardTime)
    {
        if (standardTime <= 0f)
            return 3;
        if (elapsedTime <= standardTime * threeStarMultiplier)
            return 3;
        if (elapsedTime <= standardTime * twoStarMultiplier)
            return 2;
        return elapsedTime < standardTime * oneStarMultiplier ? 1 : 0;
    }

    public float GetStarFill(int starNumber, float elapsedTime, float standardTime)
    {
        if (standardTime <= 0f)
            return 1f;

        float start;
        float end;
        switch (starNumber)
        {
            case 3: start = 0f; end = threeStarMultiplier; break;
            case 2: start = threeStarMultiplier; end = twoStarMultiplier; break;
            case 1: start = twoStarMultiplier; end = oneStarMultiplier; break;
            default: return 0f;
        }
        start *= standardTime;
        end *= standardTime;
        if (elapsedTime < start)
            return 1f;
        if (elapsedTime > end || (starNumber == 1 && elapsedTime >= end))
            return 0f;
        // Keep the star visible at an inclusive scoring boundary.
        return end > start ? Mathf.Max(0.001f, Mathf.Clamp01((end - elapsedTime) / (end - start))) : 0.001f;
    }
}
