using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetterRover : MonoBehaviour
{
    [SerializeField] BetterHand[] hands;

    public void TryExtend(Direction direction)
    {
        hands[(int)direction].TryExtend();
    }

    public void TryGrab(Direction direction)
    {
        hands[(int)direction].TryGrab();
    }
}
