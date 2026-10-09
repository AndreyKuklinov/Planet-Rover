using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarContainer : MonoBehaviour
{
    [SerializeField] Image starPrefab;
    private readonly List<Image> stars = new List<Image>();

    // Lobby ratings use whole stars; gameplay uses three persistent filled images.
    public void SetStars(int starCount)
    {
        EnsureCount(starCount);
        foreach (var star in stars)
        {
            star.type = Image.Type.Simple;
            star.fillAmount = 1f;
        }
    }

    public void SetProgress(float first, float second, float third)
    {
        EnsureCount(3);
        SetFill(stars[0], first);
        SetFill(stars[1], second);
        SetFill(stars[2], third);
    }

    private void EnsureCount(int count)
    {
        if (stars.Count == count)
            return;
        foreach (var star in stars)
            Destroy(star.gameObject);
        stars.Clear();
        for (var i = 0; i < count; i++)
        {
            var star = Instantiate(starPrefab, transform);
            star.raycastTarget = false;
            stars.Add(star);
        }
    }

    private static void SetFill(Image star, float amount)
    {
        star.type = Image.Type.Filled;
        star.fillMethod = Image.FillMethod.Horizontal;
        star.fillOrigin = (int)Image.OriginHorizontal.Left;
        star.fillAmount = Mathf.Clamp01(amount);
    }
}
