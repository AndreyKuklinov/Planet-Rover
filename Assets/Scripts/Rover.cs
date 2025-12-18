using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rover : MonoBehaviour
{
    [SerializeField] LevelObject levelObject;
    [SerializeField] Mover mover;
    [SerializeField] float roverSpeed;
    [SerializeField] LevelGrid levelGrid;
    [SerializeField] RoverArm[] arms = new RoverArm[4];

    private RoverArm movementArm;

    void Update()
    {
        if (mover.IsAtDestination)
            OnReachedDestination();
    }

    public void Extend(Direction direction)
    {
        GetArm(direction).Extend();
    }

    public void Grab(Direction direction)
    {
        var arm = GetArm(direction);
        arm.MoveHandToTarget();
        var handCell = levelGrid.WorldToCell(arm.HandPosition);

        if (arm.IsHoldingObject)
        {
            var target = levelGrid.Objects.GetObject(handCell);
            if(target == null)
            {
                arm.DropObject();
                return;
            }

            if (target.CanBeDroppedOnThis(arm.GrabbedObject))
                arm.DropObjectOnto(target);
            else
                arm.RetractHand();
        }

        else
        {
            if (levelGrid.Objects.IsEmpty(handCell))
            {
                MoveToHand(arm);
                return;
            }

            var obj = levelGrid.Objects.GetObject(handCell);

            if (obj.CanBeGrabbed)
                arm.GrabObject(obj);

            else
                arm.RetractHand();
        }
    }

    void MoveToHand(RoverArm arm)
    {
        if (arm.CurrentState != RoverArm.HandState.Extending)
            return;

        if (movementArm != null)
            movementArm.Deactivate();
        else
            levelGrid.Objects.Remove(levelObject);

        movementArm = arm;
        movementArm.GrabEmpty();
        var target = levelGrid.SnapToGrid(movementArm.HandPosition);
        mover.MoveToPosition(target, roverSpeed);
    }

    RoverArm GetArm(Direction direction)
        => arms[(int)direction];

    void OnReachedDestination()
    {
        if (movementArm == null)
            return;

        movementArm.Deactivate();
        movementArm = null;
        mover.StopMoving();
        levelObject.AttachToGrid();
    }
}
