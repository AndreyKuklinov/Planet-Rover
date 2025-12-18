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
        Holding
    }

    [field: SerializeField] public Direction Direction { get; private set; }
    [field: SerializeField] public Hand Hand { get; private set; }

    [SerializeField] float extendSpeed;
    [SerializeField] float retractSpeed;

    public HandState CurrentState { get; private set; }

    private LevelObject grabbedObject;

    public bool IsHoldingObject
        => grabbedObject != null;

    public bool IsHandOut
        => CurrentState != HandState.None;

    public Vector3 HandPosition
        => Hand.transform.position;

    void Update()
    {
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
        Hand.Mover.MoveInDirection(Direction, extendSpeed);
        CurrentState = HandState.Extending;
    }

    public void Deactivate()
    {
        Hand.gameObject.SetActive(false);
        Hand.transform.position = transform.position;
        Hand.transform.SetParent(transform);
        CurrentState = HandState.None;
    }

    public void RetractHand()
    {
        Hand.Mover.MoveToTransform(transform, retractSpeed);
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
        CurrentState = HandState.Retracting;
    }
}
