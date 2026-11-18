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
    enum DropAnimation { None, Tucking, Returning }
    DropAnimation dropAnimation;
    PlayerDevice dropPlayer;
    GridObject droppedObject;
    Transform dropVisual;
    Transform dropParent;
    Vector3 dropCellPosition;
    Vector3 dropStartHandPosition;
    Vector3 dropStartObjectPosition;
    float dropTravel;
    readonly List<SpriteRenderer> hiddenDropSprites = new();
    public Direction Direction => hand.Direction;
    bool HandIsIdle => hand.HandState == HandState.None && !hand.IsMovingRover;

    public bool IsFullyTuckedBy(PlayerDevice player)
        => isActiveAndEnabled && HandIsIdle && player != null && dropPlayer == player
            && dropAnimation == DropAnimation.Tucking && hasRenderedPosition && renderedDistance == 0f;

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
        hand.DropPressed += OnDropPressed;
        hand.ObjectDropped += OnObjectDropped;
    }

    void OnDisable()
    {
        hand.DropPressed -= OnDropPressed;
        hand.ObjectDropped -= OnObjectDropped;
        FinishDropPlacement();
        hand.IsDropTucking = false;
        dropAnimation = DropAnimation.None;
        dropPlayer = null;
    }

    void OnDropPressed(PlayerDevice player)
    {
        dropPlayer = player;
        dropAnimation = DropAnimation.Tucking;
        hand.IsDropTucking = true;
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
        var direction = DirectionVector.GetVector3(hand.Direction);
        var origin = hand.transform.position;
        var targetDistance = Mathf.Max(hand.CurrentDistance, HandRestingDistance);
        if (!hasRenderedPosition)
        {
            renderedDistance = targetDistance;
            hasRenderedPosition = true;
        }

        bool isPlacingDrop = dropVisual != null && UpdateDropPlacement(origin, direction, deltaTime);
        if (isPlacingDrop)
        {
            isVisuallyRetracting = false;
        }
        else if (HandIsIdle && dropAnimation != DropAnimation.None && !hand.IsHoldingObject)
        {
            UpdateDropAnimation(targetDistance, deltaTime);
            spriteRenderer.transform.position = origin + renderedDistance * direction;
            isVisuallyRetracting = false;
        }
        else if (hand.IsMovingRover)
        {
            spriteRenderer.transform.position = hand.LastGrabbedPos;
            renderedDistance = Mathf.Max(HandRestingDistance, Vector3.Dot(
                hand.LastGrabbedPos - origin, direction));
            isVisuallyRetracting = false;
        }
        else
        {
            // A blocked drop leaves the cargo in view at its usual holding distance.
            if (HandIsIdle && hand.IsHoldingObject && dropAnimation == DropAnimation.Tucking
                && (dropPlayer == null || !dropPlayer.IsDropHeld))
            {
                EndDropTuck();
                dropAnimation = DropAnimation.None;
            }

            isVisuallyRetracting = renderedDistance > targetDistance
                && (isVisuallyRetracting || hand.HandState != HandState.Retracting);
            renderedDistance = isVisuallyRetracting
                ? Mathf.MoveTowards(renderedDistance, targetDistance, Config.SnapRetractSpeed * deltaTime)
                : targetDistance;
            spriteRenderer.transform.position = origin + renderedDistance * direction;
        }

        spriteRenderer.enabled = renderedDistance >= Config.VisibleDistance
            || isPlacingDrop
            || hand.IsHoldingObject
            || hand.IsMovingRover;
    }

    void UpdateDropAnimation(float targetDistance, float deltaTime)
    {
        if (dropAnimation == DropAnimation.Tucking)
        {
            if (renderedDistance > targetDistance)
            {
                renderedDistance = Mathf.MoveTowards(renderedDistance, targetDistance, Config.SnapRetractSpeed * deltaTime);
                return;
            }

            renderedDistance = Mathf.MoveTowards(renderedDistance, 0f, Config.DropTuckSpeed * deltaTime);
            if (renderedDistance == 0f && (dropPlayer == null || !dropPlayer.IsDropHeld))
                EndDropTuck();
            return;
        }

        renderedDistance = Mathf.MoveTowards(renderedDistance, targetDistance, Config.DropReturnSpeed * deltaTime);
        if (renderedDistance == targetDistance)
            dropAnimation = DropAnimation.None;
    }

    void EndDropTuck()
    {
        dropPlayer = null;
        dropAnimation = DropAnimation.Returning;
        hand.IsDropTucking = false;
    }

    void OnObjectDropped(GridObject obj)
    {
        FinishDropPlacement();
        droppedObject = obj;
        dropParent = obj.transform.parent;
        dropCellPosition = obj.transform.position;
        dropStartHandPosition = spriteRenderer.transform.position;
        dropStartObjectPosition = objectHolder.position;
        dropTravel = 0f;

        // Render a temporary copy while the actual object is already at the drop cell.
        dropVisual = new GameObject("Dropping Cargo").transform;
        dropVisual.SetParent(objectHolder, false);
        foreach (var source in obj.GetComponentsInChildren<SpriteRenderer>())
        {
            if (!source.enabled || source.forceRenderingOff || source.sprite == null) continue;
            var copy = new GameObject(source.name).AddComponent<SpriteRenderer>();
            copy.transform.SetParent(dropVisual, false);
            copy.transform.SetPositionAndRotation(
                dropStartObjectPosition + (source.transform.position - dropCellPosition), source.transform.rotation);
            var scale = source.transform.lossyScale;
            var parentScale = dropVisual.lossyScale;
            copy.transform.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            copy.sprite = source.sprite;
            copy.sharedMaterial = source.sharedMaterial;
            copy.color = source.color;
            copy.flipX = source.flipX;
            copy.flipY = source.flipY;
            copy.sortingLayerID = source.sortingLayerID;
            copy.sortingOrder = source.sortingOrder;
            source.forceRenderingOff = true;
            hiddenDropSprites.Add(source);
        }
    }

    bool UpdateDropPlacement(Vector3 origin, Vector3 direction, float deltaTime)
    {
        if (droppedObject == null || droppedObject.transform.parent != dropParent
            || droppedObject.transform.position != dropCellPosition)
        {
            FinishDropPlacement();
            return false;
        }

        float distance = Mathf.Max(Vector3.Distance(dropStartHandPosition, dropCellPosition),
            Vector3.Distance(dropStartObjectPosition, dropCellPosition));
        dropTravel = Mathf.MoveTowards(dropTravel, distance, Config.SnapRetractSpeed * deltaTime);
        float progress = distance > 0f ? dropTravel / distance : 1f;
        var handPosition = Vector3.Lerp(dropStartHandPosition, dropCellPosition, progress);
        spriteRenderer.transform.position = handPosition;
        renderedDistance = Mathf.Max(0f, Vector3.Dot(handPosition - origin, direction));
        dropVisual.position = Vector3.Lerp(dropStartObjectPosition, dropCellPosition, progress);
        if (progress >= 1f) FinishDropPlacement();
        return true;
    }

    void FinishDropPlacement()
    {
        foreach (var source in hiddenDropSprites)
            if (source != null) source.forceRenderingOff = false;
        hiddenDropSprites.Clear();
        if (dropVisual == null) return;
        dropVisual.gameObject.SetActive(false);
        Destroy(dropVisual.gameObject);
        dropVisual = null;
        droppedObject = null;
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
