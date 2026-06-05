using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjective
{
    public bool IsFinal { get; }
    public void CompleteObjective();
}
