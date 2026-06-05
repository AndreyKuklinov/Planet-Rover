using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjective
{
    public static event Action<IObjective> ObjectiveCompleted;
    public static event Action<IObjective> ObjectiveCreated;

    public bool IsFinal { get; }
}
