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
    RoomResetController resetController;
    HandResetProgressIndicator resetIndicator;

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

    void OnDisable()
    {
        if (resetIndicator != null) resetIndicator.gameObject.SetActive(false);
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
        UpdateResetCharge();
    }

    void UpdateResetCharge()
    {
        if (resetController == null)
            resetController = GetComponentInParent<RoomLoader>()?.ResetController;
        float progress = 0f;
        bool visible = resetController != null && resetController.TryGetHandCharge(hand.Direction, out progress);
        if (!visible)
        {
            if (resetIndicator != null) resetIndicator.gameObject.SetActive(false);
            return;
        }
        if (resetIndicator == null)
        {
            var indicator = new GameObject("Reset Charge", typeof(RectTransform), typeof(Canvas));
            indicator.transform.SetParent(transform, false);
            var canvas = indicator.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingLayerName = "UI";
            canvas.sortingOrder = 1;
            var fill = new GameObject("Progress", typeof(RectTransform));
            fill.transform.SetParent(indicator.transform, false);
            resetIndicator = fill.AddComponent<HandResetProgressIndicator>();
            resetIndicator.color = Color.white;
            resetIndicator.raycastTarget = false;
        }
        resetIndicator.gameObject.SetActive(true);
        resetIndicator.rectTransform.sizeDelta = Vector2.one * Config.RoomResetIndicatorSize;
        var canvasTransform = resetIndicator.canvas.transform;
        ((RectTransform)canvasTransform).sizeDelta = resetIndicator.rectTransform.sizeDelta;
        canvasTransform.SetPositionAndRotation(spriteRenderer.bounds.center, Quaternion.identity);
        resetIndicator.SetProgress(progress);
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
