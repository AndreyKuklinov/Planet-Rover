using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverObject : LevelObject
{
    public override bool CanBeGrabbed => false;

    public override bool CanBeDroppedOnto(LevelObject levelObject)
        => false;

    public override HandMovementType GetHandMovementType(RoverArm arm)
        => HandMovementType.GoThrough;
}
