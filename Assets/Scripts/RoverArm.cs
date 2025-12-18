using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    public enum HandState
    {
        None,
        Extending,
        Retracting,
        MovingRover,
        Holding
    }

    [field: SerializeField] public Direction Direction { get; private set; }
    [field: SerializeField] public Hand Hand { get; private set; }

    [SerializeField] LevelGrid levelGrid;
    [SerializeField] Transform target;
    [SerializeField] float holdingDistance;
    [SerializeField] float targetSpeeed;
    [SerializeField] float handSpeed;

    public HandState CurrentState { get; private set; }

    private float targetDistance;
    private LevelObject grabbedObject;

    public bool IsHoldingObject
        => grabbedObject != null;

    public bool IsHandOut
        => CurrentState != HandState.None;

    public Vector3 HandPosition
        => Hand.transform.position;

    void Update()
    {
        if (CurrentState == HandState.Extending)
        {
            targetDistance += targetSpeeed * Time.deltaTime;
        }
        
        var transformCell = levelGrid.WorldToCell(transform.position);
        var stoppingSquare = levelGrid.GetStoppingSquare(transformCell, Direction, grabbedObject);
        var maxDistance = (levelGrid.CellToWorld(stoppingSquare) - transform.position).magnitude;

        if (transformCell == stoppingSquare)
            maxDistance = 0;

        targetDistance = Mathf.Min(targetDistance, maxDistance);
        target.transform.position = transform.position + DirectionVector.GetVector3(Direction) * targetDistance;

        if (IsHoldingObject && CurrentState == HandState.Retracting && Hand.Mover.IsAtDestination)
        {
            Hand.Mover.StopMoving();
            CurrentState = HandState.Holding;
        }

        if(CurrentState == HandState.Retracting && Hand.Mover.IsAtDestination)
        {
            Deactivate();
        }
    }

    public void Extend()
    {
        if (CurrentState == HandState.Extending || CurrentState == HandState.Retracting)
            return;
        
        Hand.gameObject.SetActive(true);
        Hand.Mover.MoveToTransform(target, handSpeed);
        CurrentState = HandState.Extending;
    }

    public void Deactivate()
    {
        Hand.gameObject.SetActive(false);
        Hand.transform.position = transform.position;
        Hand.transform.SetParent(transform);
        targetDistance = 0;
        CurrentState = HandState.None;
    }

    public void RetractHand()
    {
        if (IsHoldingObject)
        {
            targetDistance = holdingDistance;
        }

        else
        {
            targetDistance = 0;
        }

        CurrentState = HandState.Retracting;
    }

    public void GrabObject(LevelObject obj)
    {
        if (CurrentState != HandState.Extending)
            return;

        obj.AttachToObject(Hand.transform);
        grabbedObject = obj;
        RetractHand();
    }

    public void DropObject()
    {
        if (grabbedObject == null)
            throw new InvalidOperationException("Trying to drop a non-existant object");

        if (CurrentState != HandState.Extending)
            return;

        grabbedObject.AttachToGrid();
        grabbedObject = null;
        RetractHand();
    }

    public void GrabEmpty()
    {
        if (CurrentState != HandState.Extending)
            return;

        Hand.Mover.StopMoving();
        Hand.transform.SetParent(null);
        targetDistance = 0;
        CurrentState = HandState.MovingRover;
    }
}
