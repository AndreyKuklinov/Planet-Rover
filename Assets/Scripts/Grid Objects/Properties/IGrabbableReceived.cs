using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbableReceived
{
    public bool CanReceive(IGrabbable grabbedObject);
    public void Receive(IGrabbable grabbedObject);
}
