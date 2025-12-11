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

    [SerializeField] Hand hand;
    [SerializeField] float handSpeed;

    public HandState CurrentState { get; private set; }

    private LevelObject grabbedObject;

    public bool IsHoldingObject
        => grabbedObject != null;

    public bool IsHandOut
        => CurrentState != HandState.None;

    public Vector3 Target
        => hand.Target.position;

    public Vector3 HandPosition
        => hand.transform.position;

    void Update()
    {
        if (IsHoldingObject && CurrentState == HandState.Retracting && hand.Mover.IsAtDestination)
        {
            hand.Mover.StopMoving();
            CurrentState = HandState.Holding;
        }
    }

    public void Extend()
    {
        if (CurrentState == HandState.Extending || CurrentState == HandState.Retracting)
            return;

        hand.gameObject.SetActive(true);
        hand.Mover.MoveInDirection(Direction, handSpeed);
        CurrentState = HandState.Extending;
    }

    public void Deactivate()
    {
        hand.gameObject.SetActive(false);
        hand.transform.position = transform.position;
        hand.transform.SetParent(transform);
        CurrentState = HandState.None;
    }

    public void RetractHand()
    {
        hand.Mover.MoveToTransform(transform, handSpeed);
        CurrentState = HandState.Retracting;
    }

    public void GrabObject(LevelObject obj)
    {
        if (CurrentState != HandState.Extending)
            return;

        obj.AttachToObject(hand.transform);
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
        Deactivate();
    }

    public void GrabEmpty()
    {
        if (CurrentState != HandState.Extending)
            return;

        hand.Mover.StopMoving();
        hand.transform.SetParent(null);
        CurrentState = HandState.Retracting;
    }
}
