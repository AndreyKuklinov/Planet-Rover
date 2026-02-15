using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textMesh;
    [SerializeField] GameManager gm;

    void Update()
    {
        textMesh.text = gm.Score + " (" + (int)(gm.SecondsLeft) + ")";
    }
}
