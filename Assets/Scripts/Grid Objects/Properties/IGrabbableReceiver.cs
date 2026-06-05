using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbableReceiver
{
    public bool CanReceive(IGrabbable grabbedObject);
    public IGrabbable Receive(IGrabbable grabbedObject);
}
