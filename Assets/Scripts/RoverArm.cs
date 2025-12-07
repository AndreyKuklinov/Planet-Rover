using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoverArm : MonoBehaviour
{
    [field: SerializeField] public Direction Direction { get; private set; }

    [SerializeField] bool canVoluntarilyStop = true;
    [SerializeField] Mover handPrefab;
    [SerializeField] float handSpeed;

    private Mover hand;
    bool moveMyHandPlease;

    public bool IsHandExtended
        => hand != null;

    public Vector3 HandPosition
        => hand.transform.position;

    public void Extend()
    {
        if(hand == null)
        {
            hand = Instantiate(handPrefab, transform);
        }

        hand.MoveInDirection(Direction, handSpeed);
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
