using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour
{
    void Start()
    {
        AttachToGrid();
    }

    public void AttachToGrid()
    {
        var grid = FindObjectOfType<LevelGrid>();

        if (grid == null)
            return;

        grid.PlaceObject(this);
    }
}
