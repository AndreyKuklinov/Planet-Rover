using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmManager : MonoBehaviour
{
    [SerializeField] RoverArm[] arms = new RoverArm[4];

    public void Extend(Direction direction)
    {
        GetArm(direction).Extend();
    }

    public void Grab(Direction direction)
    {
        GetArm(direction).Grab();
    }

    RoverArm GetArm(Direction direction)
        => arms[(int)direction];
}
