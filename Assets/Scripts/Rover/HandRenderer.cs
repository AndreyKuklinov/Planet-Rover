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
            spriteRenderer.transform.position = hand.VisualHandPosition;
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
