using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour
{
    public abstract HandMovementType GetHandMovementType(RoverArm arm);
    public abstract bool CanBeDroppedOnto(LevelObject levelObject);
    public abstract bool CanBeGrabbed { get; }

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
        transform.position = target.position;
        grid.RemoveObject(this);
    }
}
