using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
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

        if (grabbingArm == arm || !arm.IsHandExtended)
            return;

        arm.Grab();

        if(grabbingArm != null)
            grabbingArm.Deactivate();


        grabbingArm = GetArm(direction);
    }

    RoverArm GetArm(Direction direction)
        => arms[(int)direction];
}
