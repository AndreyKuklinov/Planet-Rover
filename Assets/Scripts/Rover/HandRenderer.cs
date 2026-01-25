using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    [SerializeField] BetterHand hand;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float visibleDistance;

    void Start()
    {
        spriteRenderer.transform.rotation = Quaternion.LookRotation(
            Vector3.forward, 
            DirectionVector.GetVector3(hand.Direction)
        );
    }

    void LateUpdate()
    {
        spriteRenderer.enabled = hand.CurrentDistance >= visibleDistance 
            || hand.IsHoldingObject
            || hand.IsMovingRover;

        if (hand.IsMovingRover)
            spriteRenderer.transform.position = hand.LastGrabbedPos;
        else
            spriteRenderer.transform.position = hand.HandPosition;
    }
}
