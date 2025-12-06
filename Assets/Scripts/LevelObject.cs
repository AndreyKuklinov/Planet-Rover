using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour
{
    void Start()
    {
        var grid = FindObjectOfType<LevelGrid>();
        grid.PlaceObject(this);
    }
}
