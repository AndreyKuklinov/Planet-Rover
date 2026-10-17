using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoverHand : MonoBehaviour
{
    public static event Action<RoverHand, Vector3> SelectedMovementTarget;
    public event Action<IGrabbable> GrabbedObject;

    [field: SerializeField] public Direction Direction { get; private set; }

    public IGrabbable HeldObject { get; private set; }
    public float CurrentDistance { get; private set; } = 0;
    public HandState HandState { get; private set; }
    public Vector3 LastGrabbedPos { get; private set; }

    [SerializeField] Rover rover;
    [SerializeField] float extendSpeed;
    [SerializeField, Min(0.01f)]
    [Tooltip("Extension speed while passing through an object the hand can currently interact with or grab.")]
    float interactablePassThroughSpeed = 4f;
    [SerializeField] float retractSpeed;
    [SerializeField] bool isAutoRetractEnabled;

    Vector2Int? pendingMovementCell;

    public RoomGrid LevelGrid
        => rover.LevelGrid;

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

        var roverCell = LevelGrid.WorldToCell(transform.position);
        var nextCell = roverCell + DirectionVector.GetVector2Int(Direction);
        var objectInNextCell = LevelGrid.Objects.GetObject(nextCell);

        if (objectInNextCell != null || !LevelGrid.IsWithinBounds(nextCell))
            return;

        var nextCellPos = LevelGrid.CellToWorld(nextCell);
        HeldObject.GridObject.transform.position = nextCellPos;
        HeldObject.GridObject.AttachToGrid();
        HeldObject = null;
    }

    public void TryInteract()
    {
        if (HandState == HandState.Retracting || IsMovingRover || pendingMovementCell.HasValue)
            return;

        LastGrabbedPos = HandPosition;

        var cell = LevelGrid.WorldToCell(HandPosition);
        var obj = LevelGrid.Objects.GetObject(cell);

        if (obj == null)
        {
            MoveToHand();
            return;
        }

        if(obj.TryGetComponent<IGrabbable>(out var grab) && grab.CanBeGrabbed)
        {            
            SwitchOrGrab(grab);
            return;
        }

        if(obj.TryGetComponent<IInteractable>(out var rec) && rec.CanInteractWith(HeldObject))
        {
            PlaceHeldOntoObject(rec);
            return;
        }

        if (obj.TryGetComponent<IPassable>(out var pass) && pass.CanHandPassThrough
            && LevelGrid.TryGetNextFreeSquare(cell, Direction, out var freeCell))
        {
            pendingMovementCell = freeCell;
            HandState = HandState.Extending;
            return;
        }

        StartRetracting();
    }

    void SwitchOrGrab(IGrabbable obj)
    {
        if (!obj.CanBeGrabbed)
            throw new ArgumentException("Trying to grab an impossible object: " + obj);

        if (IsHoldingObject)
        {
            var prevObject = HeldObject;
            Grab(obj);
            prevObject.GridObject.AttachToGrid();
            return;
        }

        Grab(obj);
    }

    void MoveToHand()
    {
        LastGrabbedPos = HandPosition;
        SelectedMovementTarget?.Invoke(this, HandPosition);
        RetractInstantly();
    }

    void Grab(IGrabbable obj)
    {
        HeldObject = obj;
        GrabbedObject?.Invoke(obj);
        StartRetracting();
    }

    void PlaceHeldOntoObject(IInteractable obj)
    {
        if (!obj.CanInteractWith(HeldObject))
            throw new ArgumentException("Invalid object to place");

        HeldObject = obj.InteractWith(HeldObject);
        StartRetracting();
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

        CurrentDistance += GetExtensionSpeed() * Time.deltaTime;

        var cell = LevelGrid.WorldToCell(transform.position);
        var stoppingCell = pendingMovementCell ?? LevelGrid.GetStoppingSquare(cell, Direction, HeldObject);
        var maxDist = (LevelGrid.CellToWorld(stoppingCell) - transform.position).magnitude;

        CurrentDistance = Mathf.Min(CurrentDistance, maxDist);

        if (pendingMovementCell.HasValue && CurrentDistance >= maxDist)
        {
            MoveToHand();
            return;
        }

        if (isAutoRetractEnabled && CurrentDistance >= maxDist)
            TryInteract();
    }

    float GetExtensionSpeed()
    {
        var cell = LevelGrid.WorldToCell(HandPosition);
        var obj = LevelGrid.Objects.GetObject(cell);

        if (obj != null && obj.TryGetComponent<IPassable>(out var pass) && pass.CanHandPassThrough
            && ((obj.TryGetComponent<IInteractable>(out var interactable) && interactable.CanInteractWith(HeldObject))
                || (obj.TryGetComponent<IGrabbable>(out var grabbable) && grabbable.CanBeGrabbed)))
            return interactablePassThroughSpeed;

        return extendSpeed;
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
        pendingMovementCell = null;
        HandState = HandState.None;
        CurrentDistance = 0;
    }

    void StartRetracting()
    {
        HandState = HandState.Retracting;
    }
}
