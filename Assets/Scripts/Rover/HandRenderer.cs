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
    [SerializeField] float emptyHandRestingDistance;
    [SerializeField] Sprite[] handSprites = new Sprite[4];

    private int playerIndex;

    Vector3 TargetPosition
        => hand.transform.position
            + Mathf.Max(hand.CurrentDistance, HandRestingDistance)
            * DirectionVector.GetVector3(hand.Direction);

    float HandRestingDistance
        => hand.IsHoldingObject ? holdingDistance : emptyHandRestingDistance;

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
        UpdateHandPosition();
        UpdateHandSprite();
        UpdateLine();
    }
    void UpdateHandPosition()
    {
        float renderDistance = Mathf.Max(hand.CurrentDistance, HandRestingDistance);

        spriteRenderer.enabled = renderDistance >= visibleDistance
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

    void UpdateHandSprite()
    {
        var index = PartyInputManager.Instance.DirectionToPlayerIndex[hand.Direction];
        if (playerIndex == index)
            return;

        playerIndex = index;
        spriteRenderer.sprite = handSprites[playerIndex];
    }

    void OnHandGrabbedObject(IGrabbable obj)
    {
        obj.GridObject.AttachToObject(objectHolder);
    }
}
