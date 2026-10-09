using UnityEngine;

// Legacy portals remain in room geometry but no longer complete or skip rooms.
public class RoomExit : MonoBehaviour, IInteractable
{
    public bool CanInteractWith(IGrabbable grabbedObject) => false;
    public IGrabbable InteractWith(IGrabbable grabbedObject) => grabbedObject;
}