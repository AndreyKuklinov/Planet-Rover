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

    [SerializeField] protected SpriteRenderer spriteRenderer;

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

    public LevelGrid LevelGrid { get; private set; }

    public void AttachToGrid(LevelGrid grid = null)
    {
        if (grid != null)
            LevelGrid = grid;
        else if (LevelGrid == null)
            throw new ArgumentException("Level objects need a level grid when first created");

        transform.SetParent(LevelGrid.transform);
        LevelGrid.PlaceObject(this);

        if(startingCell == null)
            startingCell = LevelGrid.Objects.GetPosition(this);

        spriteRenderer.sortingOrder = 0;
    }
    
    public void AttachToObject(Transform target)
    {
        transform.SetParent(target.transform);
        transform.position = target.position;
        LevelGrid.RemoveObject(this);
        spriteRenderer.sortingOrder = 1;
    }

    public void MoveToPosition(Vector2 destination)
    {
        if(!IsMoving)
            LevelGrid.RemoveObject(this);

        movementDestination = destination;
        IsMoving = true;
    }

    public void Remove()
    {
        Destroy(gameObject);
    }

    //public void ReturnToSpawn()
    //{
    //    var occupyingObject = LevelGrid.Objects.GetObject(startingCell);
    //    if (occupyingObject != null)
    //    {
    //        LevelGrid.RemoveObject(occupyingObject);
    //        occupyingObject.ReturnToSpawn();
    //    }

    //    transform.position = LevelGrid.CellToWorld(startingCell);
    //    AttachToGrid();
    //}

    protected virtual void Start() {}
    protected virtual void OnDestroy() { }

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
