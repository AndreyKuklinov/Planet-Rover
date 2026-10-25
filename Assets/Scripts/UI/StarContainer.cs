using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarContainer : MonoBehaviour
{
    [SerializeField] Image starPrefab;
    private readonly List<Image> stars = new List<Image>();

    // Lobby and gameplay ratings use whole stars.
    public void SetStars(int starCount, bool showLastStar = true)
    {
        EnsureCount(starCount);
        for (int i = 0; i < stars.Count; i++)
        {
            var star = stars[i];
            star.type = Image.Type.Simple;
            star.fillAmount = 1f;
            // Keep the layout slot in place while its icon blinks.
            star.enabled = i < stars.Count - 1 || showLastStar;
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
