using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour 
{
    Vector2Int startingCell;

    public virtual bool CanHandGoThrough
        => false;

    public virtual bool CanBeGrabbed
        => false;

    public virtual bool CanReceive(LevelObject levelObject)
        => false;

    public virtual void Receive(LevelObject levelObject)
    {
        throw new NotImplementedException();
    }

    protected LevelGrid grid;

    protected virtual void Start()
    {
        grid = FindObjectOfType<LevelGrid>();
        AttachToGrid();
        startingCell = grid.Objects.GetPosition(this);
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

    public void Remove()
    {
        Destroy(gameObject);
    }

    public void ReturnToSpawn()
    {
        var occupyingObject = grid.Objects.GetObject(startingCell);
        if (occupyingObject != null)
        {
            grid.RemoveObject(occupyingObject);
            occupyingObject.ReturnToSpawn();
        }

        transform.position = grid.CellToWorld(startingCell);
        AttachToGrid();
    }
}
