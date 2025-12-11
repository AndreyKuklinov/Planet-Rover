using System.Collections;
using System.Collections.Generic;
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
                MoveToHand(arm);

            else
                arm.GrabObject(levelGrid.Objects.GetObject(handCell));
        }
    }

    void MoveToHand(RoverArm arm)
    {
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
}
