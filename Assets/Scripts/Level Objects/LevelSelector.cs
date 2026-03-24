using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : LevelObject
{
    public static event Action<LevelSet> LevelSetSelected;
    [SerializeField] LevelSet levelSet;

    public override bool CanReceive(LevelObject levelObject)
    {
        return true;
    }

    public override void Receive(LevelObject levelObject)
    {
        LevelSetSelected?.Invoke(levelSet);
    }
}
