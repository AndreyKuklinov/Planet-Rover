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
        var t = "";

        for (var i = 0; i < gm.Stars; i++)
            t += "*";

        if (gm.IsTimeRunning)
            t += " (" + (int)(gm.SecondsLeft) + ")";
        textMesh.text = t;
    }
}
