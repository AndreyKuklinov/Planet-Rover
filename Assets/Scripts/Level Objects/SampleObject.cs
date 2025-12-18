using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleObject : LevelObject
{
    public override bool CanBeGrabbed => true;

    public override bool CanBeDroppedOnto(LevelObject levelObject)
        => false;

    public override HandMovementType GetHandMovementType(RoverArm arm)
        => HandMovementType.HoverOver;
}
