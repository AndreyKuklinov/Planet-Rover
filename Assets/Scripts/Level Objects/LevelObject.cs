using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour
{
    [field: SerializeField] public bool CanHandGoThrough { get; private set; }
    [field: SerializeField] public bool CanBeGrabbed { get; private set; }

    public virtual bool CanBeDroppedOnto(LevelObject levelObject)
        => false;
    

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
