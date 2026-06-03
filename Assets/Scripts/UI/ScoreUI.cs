//using System.Collections;
//using System.Collections.Generic;
//using System.Text;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class ScoreUI : MonoBehaviour
//{
//    [SerializeField] TextMeshProUGUI textMesh;
//    [SerializeField] StarContainer starContainer;
//    [SerializeField] Image powerImage;
//    [SerializeField] Transform ui;
//    [SerializeField] GameManager gm;

//    bool isVisible
//        => gm.IsTimeRunning || gm.IsGameOver;

//    void Update()
//    {
//        ui.gameObject.SetActive(isVisible);
//        if (!isVisible)
//            return;

//        textMesh.text = GetScoreText();
//        UpdateStars();
//        UpdatePower();
//    }

//    string GetScoreText()
//    {
//        return gm.Score.ToString() + GetTargetScore();
//    }

//    void UpdateStars()
//    {
//        starContainer.SetStars(gm.Stars);
//    }

//    void UpdatePower()
//    {
//        var value = gm.SecondsLeft / gm.LevelSet.GameDuration;
//        powerImage.fillAmount = Mathf.Clamp(value, 0, 1);
//    }

//    string GetTargetScore()
//    {
//        int? target = null;
//        foreach(var star in gm.LevelSet.StarThresholds)
//        {
//            if(gm.Score < star)
//            {
//                target = star;
//                break;
//            }
//        }

//        if(target == null)
//            return "";

//        return "/" + target.Value;
//    }
//}
