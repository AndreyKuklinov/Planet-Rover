using System.Collections.Generic;
using UnityEngine;

public class Rover : MonoBehaviour
{
    [SerializeField] RoverArm[] arms = new RoverArm[4];

    Queue<Directions> awaitingConstriction = new Queue<Directions>();

    public void OnArmPressed(Directions armDirection)
    {
        var arm = arms[((int)armDirection)];
        if (arm.IsHandExtended)
            return;

        arm.ExtendHand();
    }

    public void OnArmReleased(Directions armDirection)
    {

    }

    void Update()
    {

    }
}
