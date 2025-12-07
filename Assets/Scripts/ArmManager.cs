using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
    [SerializeField] LevelGrid levelGrid;
    [SerializeField] RoverArm[] arms = new RoverArm[4];

    private RoverArm grabbingArm;

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

        if (!arm.IsHandExtended)
            return;

        arm.Grab();
    }

    RoverArm GetArm(Direction direction)
        => arms[(int)direction];
}
