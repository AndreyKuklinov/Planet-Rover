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
        transform.rotation = Quaternion.LookRotation(
            Vector3.forward, 
            DirectionVector.GetVector3(hand.Direction)
        );
    }

    void Update()
    {
        spriteRenderer.enabled = hand.CurrentDistance >= visibleDistance || hand.IsHoldingObject;
        transform.position = hand.HandPosition;
    }
}
