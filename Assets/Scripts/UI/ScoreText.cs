using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] GameManager gm;

    void Update()
    {
        textMesh.text = GetText();
    }

    string GetText()
    {
        var t = "";

        if (!gm.IsTimeRunning && !gm.IsGameWon && !gm.IsGameOver)
            return t;

        t += gm.Score.ToString();

        if (gm.IsTimeRunning)
        {
            t += " [" + (int)(gm.SecondsLeft) + "]";
            return t;
        }

        t += " [";
        for (var i = 0; i < gm.Stars; i++)
            t += "*";
        t += "]";

        return t;
    }
}
