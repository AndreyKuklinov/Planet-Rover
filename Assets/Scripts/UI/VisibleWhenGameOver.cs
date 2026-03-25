using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisibleWhenGameOver : MonoBehaviour
{
    [SerializeField] GameManager gameManager;
    [SerializeField] Image image;

    void Update()
    {
        image.enabled = gameManager.IsGameOver;
    }
}
