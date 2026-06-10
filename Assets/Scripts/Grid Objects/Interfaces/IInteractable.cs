using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public bool CanInteractWith(IGrabbable grabbedObject);
    public IGrabbable InteractWith(IGrabbable grabbedObject);
}
