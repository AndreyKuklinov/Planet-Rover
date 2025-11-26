using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rover : MonoBehaviour
{
    [SerializeField] RoverArm[] arms = new RoverArm[4];
    [SerializeField] float movementSpeed;

    //TODO: Rework queue to keep track of what arm first started extending, not retracting
    Queue<RoverArm> awaitingRetraction = new Queue<RoverArm>();
    RoverArm retractingArm = null;

    public void OnArmPressed(Directions armDirection)
    {
        var arm = arms[((int)armDirection)];
        if (arm.IsHandExtended)
            return;

        arm.ExtendHand();
    }

    public void OnArmReleased(Directions armDirection)
    {
        var arm = arms[((int)armDirection)];
        if (!arm.IsHandExtended)
            return;

        arm.StopExtending();
        awaitingRetraction.Enqueue(arm);
    }

    void Update()
    {
        UpdateRetractingArm();
        MoveTowardsHand();
    }

    void UpdateRetractingArm()
    {
        if (awaitingRetraction.Count == 0 || retractingArm != null)
            return;

        retractingArm = awaitingRetraction.Dequeue();
        retractingArm.UnchildHand();
    }

    void MoveTowardsHand()
    {
        if (retractingArm == null)
            return;

        var target = retractingArm.Hand.transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            movementSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) <= 0.001f)
        {
            retractingArm.DestroyHand();
            retractingArm = null;
        }
    }
}
