using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    [SerializeField] BetterHand hand;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float visibleDistance;

    private BetterRover rover;
    private Vector3 lastChosenMovementTarget;

    void Start()
    {
        spriteRenderer.transform.rotation = Quaternion.LookRotation(
            Vector3.forward, 
            DirectionVector.GetVector3(hand.Direction)
        );

        BetterHand.SelectedMovementTarget += OnHandSelectedMovementTarget;

        rover = FindObjectOfType<BetterRover>();
    }

    void LateUpdate()
    {
        spriteRenderer.enabled = hand.CurrentDistance >= visibleDistance 
            || hand.IsHoldingObject
            || rover.IsMoving && rover.TargetHand == hand;

        if (rover.IsMoving && rover.TargetHand == hand)
        {
            Debug.Log("huh");
            spriteRenderer.transform.position = lastChosenMovementTarget;
        }
        else
        {
            spriteRenderer.transform.position = hand.HandPosition;
        }
    }

    void OnHandSelectedMovementTarget(BetterHand h, Vector3 target)
    {
        if (h != hand)
            return;

        lastChosenMovementTarget = target;
    }
}
