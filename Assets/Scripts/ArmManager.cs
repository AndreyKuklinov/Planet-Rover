using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
    [SerializeField] LevelObject roverObj;
    [SerializeField] Mover roverBody;
    [SerializeField] float roverSpeed;
    [SerializeField] LevelGrid levelGrid;
    [SerializeField] RoverArm[] arms = new RoverArm[4];

    private RoverArm movementArm;

    void Start()
    {
        roverBody.ReachedDestination += OnRoverReachedDestination;
    }

    public void Extend(Direction direction)
    {
        var arm = GetArm(direction);

        if (arm.IsHandExtended)
            return;

        arm.Extend();
    }

    public void Grab(Direction direction)
    {
        var arm = GetArm(direction);

        if (!arm.IsHandExtended || arm.IsRetracting)
            return;

        var handCell = levelGrid.WorldToCell(arm.Target);
        if (levelGrid.Objects.IsEmpty(handCell))
            MoveToHand(arm);

        else
            arm.Deactivate();
    }

    public void MoveToHand(RoverArm arm)
    {
        if (movementArm != null)
            movementArm.Deactivate();
        else
            levelGrid.Objects.Remove(roverObj);

        movementArm = arm;
        movementArm.IsRetracting = true;

        movementArm.DetachHand();
        movementArm.StopHand();
        var target = levelGrid.SnapToGrid(movementArm.Target);
        roverBody.MoveToPosition(target, roverSpeed);
    }

    RoverArm GetArm(Direction direction)
        => arms[(int)direction];

    public void OnRoverReachedDestination()
    {
        if(movementArm != null)
        {
            roverObj.AttachToGrid();
            movementArm.Deactivate();
            movementArm = null;
        }
    }
}
