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
        var t = gm.Score.ToString();
        if (gm.IsTimeRunning)
            t += " (" + (int)(gm.SecondsLeft) + ")";
        textMesh.text = t;
    }
}
