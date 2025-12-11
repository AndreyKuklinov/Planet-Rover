using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour
{
    private LevelGrid grid;

    void Start()
    {
        grid = FindObjectOfType<LevelGrid>();
        AttachToGrid();
    }

    public void AttachToGrid()
    {
        if (grid == null)
            return;

        transform.SetParent(grid.transform);
        grid.PlaceObject(this);
    }
    
    public void AttachToObject(Transform target)
    {
        transform.SetParent(target.transform);
        grid.RemoveObject(this);
    }
}
