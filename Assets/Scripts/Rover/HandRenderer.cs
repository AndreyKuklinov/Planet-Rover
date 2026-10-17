using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandRenderer : MonoBehaviour
{
    [SerializeField] RoverHand hand;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform objectHolder;
    [SerializeField] Sprite[] handSprites = new Sprite[4];
    [SerializeField] SpriteRenderer cellHighlight;

    RoverHandConfig Config => hand.Config;

    private int playerIndex;
    float renderedDistance;
    bool hasRenderedPosition;
    bool isVisuallyRetracting;

    Vector3 TargetPosition
        => hand.transform.position
            + Mathf.Max(hand.CurrentDistance, HandRestingDistance)
            * DirectionVector.GetVector3(hand.Direction);

    float HandRestingDistance
        => hand.IsHoldingObject ? Config.HoldingDistance : Config.EmptyHandRestingDistance;

    void OnEnable()
    {
        hasRenderedPosition = false;
        isVisuallyRetracting = false;
    }

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
        UpdateHandPosition(Time.deltaTime);
        UpdateHandSprite();
        UpdateLine();
        UpdateCellHightlight();
    }
    void UpdateHandPosition(float deltaTime)
    {
        var targetDistance = Mathf.Max(hand.CurrentDistance, HandRestingDistance);
        if (!hasRenderedPosition)
        {
            renderedDistance = targetDistance;
            hasRenderedPosition = true;
        }

        if (hand.IsMovingRover)
        {
            spriteRenderer.transform.position = hand.LastGrabbedPos;
            renderedDistance = Mathf.Max(HandRestingDistance, Vector3.Dot(
                hand.LastGrabbedPos - hand.transform.position,
                DirectionVector.GetVector3(hand.Direction)));
            isVisuallyRetracting = false;
        }
        else
        {
            isVisuallyRetracting = renderedDistance > targetDistance
                && (isVisuallyRetracting || hand.HandState != HandState.Retracting);
            renderedDistance = isVisuallyRetracting
                ? Mathf.MoveTowards(renderedDistance, targetDistance, Config.SnapRetractSpeed * deltaTime)
                : targetDistance;
            spriteRenderer.transform.position = hand.transform.position
                + renderedDistance * DirectionVector.GetVector3(hand.Direction);
        }

        spriteRenderer.enabled = renderedDistance >= Config.VisibleDistance
            || hand.IsHoldingObject
            || hand.IsMovingRover;
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

    void UpdateCellHightlight()
    {
        if (hand.HandState != HandState.Extending 
            || hand.CurrentDistance < Config.VisibleDistance
            || !Config.IsCellHighlightEnabled)
        {
            cellHighlight.enabled = false;
            return;
        }
        cellHighlight.enabled = true;
        var pos = hand.LevelGrid.SnapToGrid(TargetPosition);
        cellHighlight.transform.position = pos;
    }

    void OnHandGrabbedObject(IGrabbable obj)
    {
        obj.GridObject.AttachToObject(objectHolder);
    }
}
