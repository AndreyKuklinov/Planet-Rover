using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    [SerializeField] RoverHand hand;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform objectHolder;
    [SerializeField] float visibleDistance;
    [SerializeField] float holdingDistance;

    Vector3 TargetPosition
        => hand.transform.position
            + Mathf.Max(hand.CurrentDistance, HandRestingDistance)
            * DirectionVector.GetVector3(hand.Direction);

    float HandRestingDistance
        => hand.IsHoldingObject ? holdingDistance : 0;

    void Start()
    {
        spriteRenderer.transform.rotation = Quaternion.LookRotation(
            Vector3.forward, 
            DirectionVector.GetVector3(hand.Direction)
        );

        hand.GrabbedObject += OnHandGrabbedObject;
    }

    void LateUpdate()
    {
        UpdateHand();
        UpdateLine();
    }
    void UpdateHand()
    {
        spriteRenderer.enabled = hand.CurrentDistance >= visibleDistance
            || hand.IsHoldingObject
            || hand.IsMovingRover;

        if (hand.IsMovingRover)
            spriteRenderer.transform.position = hand.LastGrabbedPos;
        else
            spriteRenderer.transform.position = TargetPosition;
    }

    void UpdateLine()
    {
        lineRenderer.enabled = spriteRenderer.enabled;
        lineRenderer.SetPositions(new[] {
            transform.position,
            spriteRenderer.transform.position,
        });
    }

    void OnHandGrabbedObject(LevelObject obj)
    {
        obj.AttachToObject(objectHolder);
    }
}
