using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidSample : GridObject, IGrabbable
{
    public GridObject GridObject =>
        this;
}
