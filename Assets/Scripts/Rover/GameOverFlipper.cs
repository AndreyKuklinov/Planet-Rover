using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverFlipper : MonoBehaviour
{
    void Start()
    {
        GameManager.GameEnded += OnGameEnded;
    }

    private void OnDestroy()
    {
        GameManager.GameEnded -= OnGameEnded;
    }

    private void OnGameEnded()
    {
        transform.eulerAngles = new Vector3(0, 0, -90);
    }
}
