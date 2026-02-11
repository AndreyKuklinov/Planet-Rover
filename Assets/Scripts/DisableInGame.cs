using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableInGame : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
