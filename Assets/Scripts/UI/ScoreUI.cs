using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] Transform starsContainer;
    [SerializeField] Image starPrefab;
    [SerializeField] Image powerImage;
    [SerializeField] Transform ui;
    [SerializeField] GameManager gm;

    private List<Image> stars = new List<Image>();

    bool isVisible
        => gm.LevelSet != null && !gm.LevelSet.IsTutorial;

    void Update()
    {
        ui.gameObject.SetActive(isVisible);
        if (!isVisible)
            return;

        textMesh.text = GetScoreText();
        UpdateStars();
        UpdatePower();
    }

    string GetScoreText()
    {
        return gm.Score.ToString();
    }

    void UpdateStars()
    {
        foreach(var star in stars)
        {
            Destroy(star.gameObject);
        }

        stars = new List<Image>();
        for(var i = 0; i < gm.Stars; i++)
        {
            var star = Instantiate(starPrefab, starsContainer);
            stars.Add(star);
        }
    }

    void UpdatePower()
    {
        var value = gm.SecondsLeft / gm.LevelSet.GameDuration;
        powerImage.fillAmount = value;
    }
}
