using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [field: SerializeField] public Direction Direction { get; private set; }

    [SerializeField] bool canVoluntarilyStop = true;
    [SerializeField] Hand handPrefab;
    [SerializeField] float handSpeed;

    public bool IsRetracting { get; private set; }

    private Hand hand;
    private LevelObject grabbedObject;

    public bool IsHoldingObject
        => grabbedObject != null;

    public bool IsHandExtended
        => hand != null;

    public Vector3 Target
        => hand.Target.position;

    public Vector3 HandPosition
        => hand.transform.position;

    void Update()
    {
        if (IsHoldingObject && IsRetracting && hand.Mover.IsAtDestination)
        {
            hand.Mover.StopMoving();
            IsRetracting = false;
        }
    }

    public void Extend()
    {
        if(hand == null)
        {
            hand = Instantiate(handPrefab, transform);
        }

        hand.Mover.MoveInDirection(Direction, handSpeed);
    }

    public void StopHand()
    {
        if (!canVoluntarilyStop)
            return;

        hand.Mover.StopMoving();
    }

    public void DetachHand()
    {
        hand.transform.SetParent(null);
    }

    public void Deactivate()
    {
        Destroy(hand.gameObject);
        IsRetracting = false;
    }

    public void GrabObject(LevelObject obj)
    {
        obj.transform.SetParent(hand.transform);
        hand.Mover.MoveToTransform(transform, handSpeed);
        grabbedObject = obj;
        IsRetracting = true;
    }

    public void DropObject()
    {
        grabbedObject.transform.SetParent(null);
        grabbedObject.AttachToGrid();
        grabbedObject = null;
        Deactivate();
    }

    public void GrabEmpty()
    {
        IsRetracting = true; 
    }
}
