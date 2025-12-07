using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [field: SerializeField] public Direction Direction { get; private set; }

    [SerializeField] bool canVoluntarilyStop = true;
    [SerializeField] Hand handPrefab;
    [SerializeField] float handSpeed;

    public bool IsRetracting;

    private Hand hand;

    public bool IsHandExtended
        => hand != null;

    public Vector3 Target
        => hand.Target.position;

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
}
