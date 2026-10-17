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
    [Tooltip("Extension speed through Interact Or Retract passable objects affected by Only Affect Interactable Passables.")]
    float interactablePassThroughSpeed = 4f;
    [SerializeField]
    [Tooltip("Apply slowdown and snap-back to Interact Or Retract passables only when the hand can currently interact with or grab them. Disable to affect all passables in that mode. Continue To Free Cell objects are always exempt.")]
    bool onlyAffectInteractablePassables = true;
    [SerializeField]
    [Tooltip("Snap an extended hand back to the cell before an affected passable object when the rover changes rows or columns.")]
    bool enablePassableSnapBack = true;
    [SerializeField] float retractSpeed;
    [SerializeField] bool isAutoRetractEnabled;

    Vector2Int? pendingMovementCell;
    Vector2Int? previousExtensionCell;

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

        previousExtensionCell = LevelGrid.WorldToCell(transform.position);
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

        if (obj.TryGetComponent<IPassable>(out var pass) && pass.CanHandPassThrough
            && pass.PassageMode == HandPassageMode.ContinueToFreeCell)
        {
            if (LevelGrid.TryGetNextFreeSquare(cell, Direction, out var freeCell))
            {
                pendingMovementCell = freeCell;
                previousExtensionCell = LevelGrid.WorldToCell(transform.position);
                HandState = HandState.Extending;
            }
            else
            {
                StartRetracting();
            }
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

        var cell = LevelGrid.WorldToCell(transform.position);
        var step = DirectionVector.GetVector2Int(Direction);
        var changedLine = previousExtensionCell.HasValue && (step.x == 0
            ? cell.x != previousExtensionCell.Value.x
            : cell.y != previousExtensionCell.Value.y);
        var needsReleaseUpdate = pendingMovementCell.HasValue && previousExtensionCell.HasValue
            && cell != previousExtensionCell.Value;
        previousExtensionCell = cell;

        if (needsReleaseUpdate)
            pendingMovementCell = null;

        var stoppingCell = pendingMovementCell ?? LevelGrid.GetStoppingSquare(cell, Direction, HeldObject);
        var maxDist = (LevelGrid.CellToWorld(stoppingCell) - transform.position).magnitude;

        CurrentDistance = Mathf.Min(CurrentDistance, maxDist);
        var snappedBack = enablePassableSnapBack && changedLine && TrySnapBackToPassable(cell);
        if (needsReleaseUpdate)
        {
            TryInteract();
            return;
        }

        if (snappedBack)
            return;

        CurrentDistance = Mathf.Min(CurrentDistance + GetExtensionSpeed() * Time.deltaTime, maxDist);

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
        if (cell == LevelGrid.WorldToCell(transform.position))
            return extendSpeed;

        var obj = LevelGrid.Objects.GetObject(cell);

        return ShouldAffectPassable(obj) ? interactablePassThroughSpeed : extendSpeed;
    }

    bool ShouldAffectPassable(GridObject obj)
    {
        return obj != null && obj.TryGetComponent<IPassable>(out var pass) && pass.CanHandPassThrough
            && pass.PassageMode == HandPassageMode.InteractOrRetract
            && (!onlyAffectInteractablePassables
                || (obj.TryGetComponent<IInteractable>(out var interactable) && interactable.CanInteractWith(HeldObject))
                || (obj.TryGetComponent<IGrabbable>(out var grabbable) && grabbable.CanBeGrabbed));
    }

    bool TrySnapBackToPassable(Vector2Int from)
    {
        var step = DirectionVector.GetVector2Int(Direction);
        var point = from + step;

        while (LevelGrid.IsWithinBounds(point))
        {
            var distance = Vector3.Dot(LevelGrid.CellToWorld(point) - transform.position,
                DirectionVector.GetVector3(Direction));
            if (distance > CurrentDistance)
                return false;

            var obj = LevelGrid.Objects.GetObject(point);
            if (ShouldAffectPassable(obj))
            {
                CurrentDistance = Mathf.Max(0f, Vector3.Dot(
                    LevelGrid.CellToWorld(point - step) - transform.position,
                    DirectionVector.GetVector3(Direction)));
                return true;
            }

            if (obj != null && (!obj.TryGetComponent<IPassable>(out var pass) || !pass.CanHandPassThrough))
                return false;

            point += step;
        }

        return false;
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
