using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoverHand : MonoBehaviour
{
    public static event Action<RoverHand, Vector3> SelectedMovementTarget;
    public event Action<LevelObject> GrabbedObject;

    [field: SerializeField] public Direction Direction { get; private set; }

    public LevelObject HeldObject { get; private set; }
    public float CurrentDistance { get; private set; } = 0;
    public HandState HandState { get; private set; }
    public Vector3 LastGrabbedPos { get; private set; }

    [SerializeField] Rover rover;
    [SerializeField] float extendSpeed;
    [SerializeField] float retractSpeed;
    [SerializeField] bool isAutoRetractEnabled;

    private LevelGrid levelGrid;

    public bool IsHoldingObject
        => HeldObject != null;

    public bool IsMovingRover
        => rover.IsMoving && rover.TargetHand == this;

    public Vector3 HandPosition
        => transform.position + CurrentDistance * DirectionVector.GetVector3(Direction);

    public void TryExtend()
    {
        if (HandState != HandState.None || IsMovingRover)
            return;

        HandState = HandState.Extending;
    }

    public void TryDrop()
    {
        if (HandState == HandState.Retracting || IsMovingRover || !IsHoldingObject)
            return;

        var roverCell = levelGrid.WorldToCell(transform.position);
        var nextCell = roverCell + DirectionVector.GetVector2Int(Direction);
        var objectInNextCell = levelGrid.Objects.GetObject(nextCell);

        if (objectInNextCell != null || !levelGrid.IsWithinBounds(nextCell))
            return;

        var nextCellPos = levelGrid.CellToWorld(nextCell);
        HeldObject.transform.position = nextCellPos;
        HeldObject.AttachToGrid();
        HeldObject = null;
    }

    public void TryInteract()
    {
        if (HandState == HandState.Retracting || IsMovingRover)
            return;

        LastGrabbedPos = HandPosition;

        var cell = levelGrid.WorldToCell(HandPosition);
        var obj = levelGrid.Objects.GetObject(cell);

        if (obj == null)
        {
            MoveToHand();
            return;
        }

        if(obj.CanBeGrabbed)
        {            
            SwitchOrGrab(obj);
            return;
        }

        if(IsHoldingObject && obj.CanReceive(HeldObject))
        {
            PlaceHeldOntoObject(obj);
            return;
        }

        StartRetracting();
    }

    void SwitchOrGrab(LevelObject obj)
    {
        if (!obj.CanBeGrabbed)
            throw new ArgumentException("Trying to grab an impossible object: " + obj);

        if (IsHoldingObject)
        {
            var prevObject = HeldObject;
            Grab(obj);
            prevObject.AttachToGrid();
            return;
        }

        Grab(obj);
    }

    void MoveToHand()
    {
        SelectedMovementTarget?.Invoke(this, HandPosition);
        RetractInstantly();
    }

    void Grab(LevelObject obj)
    {
        HeldObject = obj;
        GrabbedObject?.Invoke(obj);
        StartRetracting();
    }

    void PlaceHeldOntoObject(LevelObject obj)
    {
        if (!IsHoldingObject)
            throw new Exception("Nothing to drop");

        obj.Receive(HeldObject);
        StartRetracting();
    }

    void Start()
    {
        levelGrid = FindObjectOfType<LevelGrid>();
    }

    void Update()
    {
        UpdateExtension();
        UpdateRetraction();
    }

    void UpdateExtension()
    {
        if (HandState != HandState.Extending)
            return;

        CurrentDistance += extendSpeed * Time.deltaTime;

        var cell = levelGrid.WorldToCell(transform.position);
        var stoppingCell = levelGrid.GetStoppingSquare(cell, Direction, HeldObject);
        var maxDist = (levelGrid.CellToWorld(stoppingCell) - transform.position).magnitude;

        CurrentDistance = Mathf.Min(CurrentDistance, maxDist);

        if (isAutoRetractEnabled && CurrentDistance >= maxDist)
            TryInteract();
    }

    void UpdateRetraction()
    {
        if (HandState != HandState.Retracting)
            return;

        CurrentDistance -= retractSpeed * Time.deltaTime;

        if (CurrentDistance <= 0)
            HandState = HandState.None;
    }
    void RetractInstantly()
    {
        HandState = HandState.None;
        CurrentDistance = 0;
    }

    void StartRetracting()
    {
        HandState = HandState.Retracting;
    }
}
