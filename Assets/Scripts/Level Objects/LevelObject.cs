using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class LevelObject : MonoBehaviour 
{
    [SerializeField] float movementSpeed;
    Vector2Int startingCell;
    Vector2 movementDestination;

    public bool IsMoving { get; private set; }

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
        grid = LevelGrid.Current;
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

    public void MoveToPosition(Vector2 destination)
    {
        if(!IsMoving)
            grid.RemoveObject(this);

        movementDestination = destination;
        IsMoving = true;
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

    void Update()
    {
        MoveToTarget();
        CheckIfReachedTarget();
    }

    void MoveToTarget()
    {
        if (!IsMoving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            movementDestination,
            movementSpeed * Time.deltaTime
        );
    }

    void CheckIfReachedTarget()
    {
        if (!IsMoving)
            return;

        if (Vector3.Distance(transform.position, movementDestination) <= 0.001f)
        {
            IsMoving = false;
            AttachToGrid();
        }
    }
}
