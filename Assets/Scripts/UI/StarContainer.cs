using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarContainer : MonoBehaviour
{
    [SerializeField] Image starPrefab;
    private List<Image> stars = new List<Image>();

    public void SetStars(int starCount)
    {
        foreach (var star in stars)
        {
            Destroy(star.gameObject);
        }

        stars = new List<Image>();
        for (var i = 0; i < starCount; i++)
        {
            var star = Instantiate(starPrefab, transform);
            stars.Add(star);
        }
    }
}
