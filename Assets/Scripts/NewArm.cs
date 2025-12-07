using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [SerializeField] Direction direction;
    [SerializeField] bool canVoluntarilyStop = true;
    [SerializeField] Mover handPrefab;
    [SerializeField] float handSpeed;

    private Mover hand;

    public bool IsHandExtended
        => hand != null;

    public void Extend()
    {
        if(hand == null)
        {
            hand = Instantiate(handPrefab, transform);
        }

        hand.MoveInDirection(direction, handSpeed);
    }

    public void Grab()
    {
        if (!canVoluntarilyStop)
            return;

        hand.StopMoving();
    }

    public void Deactivate()
    {
        Destroy(hand.gameObject);
    }
}
