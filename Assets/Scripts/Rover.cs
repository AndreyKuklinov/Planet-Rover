using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.Sqlite;
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

        foreach(var arm in arms)
        {
            if (IsArmOverImpassableObjects(arm))
                arm.RetractHand();
        }
    }

    public void Extend(Direction direction)
    {
        GetArm(direction).Extend();
    }

    public void Grab(Direction direction)
    {
        var arm = GetArm(direction);

        var handCell = levelGrid.WorldToCell(arm.Target);

        if (arm.IsHoldingObject)
        {
            if (levelGrid.Objects.IsEmpty(handCell))
                arm.DropObject();
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

            if (obj.IsGrabbable)
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
        var target = levelGrid.SnapToGrid(movementArm.Target);
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

    bool IsArmOverImpassableObjects(RoverArm arm)
    {
        var armPos = levelGrid.WorldToCell(arm.transform.position);
        var handPos = levelGrid.WorldToCell(arm.HandPosition);
        return levelGrid.GetObjectsOnLine(armPos, handPos).Any(x => x.IsImpassable);
    }
}
