using UnityEngine;

[CreateAssetMenu(fileName = "Rover Hand Config", menuName = "Rover/Hand Config")]
public class RoverHandConfig : ScriptableObject
{
    [Header("Room Reset")]
    [SerializeField, Min(0f)]
    [Tooltip("Hold drop this long before the hand starts charging and shows its reset circle.")]
    float roomResetHoldThreshold = 0.3f;
    [SerializeField, Min(0.01f)]
    [Tooltip("Time to fully charge after the hold threshold. Releasing drop clears the charge, even when full.")]
    float roomResetHoldDuration = 2f;
    [SerializeField, Min(0.01f)] float roomResetIndicatorSize = 0.35f;
    public float RoomResetHoldThreshold => roomResetHoldThreshold;
    public float RoomResetHoldDuration => roomResetHoldDuration;
    public float RoomResetIndicatorSize => roomResetIndicatorSize;

    [Header("Movement")]
    [SerializeField, Min(0.01f)] float extendSpeed = 12f;
    [SerializeField, Min(0.01f)] float retractSpeed = 30f;
    [SerializeField] bool isAutoRetractEnabled;

    [Header("Passable Objects")]
    [SerializeField, Min(0.01f)]
    [Tooltip("Extension speed through Interact Or Retract passable objects affected by Only Affect Interactable Passables.")]
    float interactablePassThroughSpeed = 2f;
    [SerializeField]
    [Tooltip("Apply slowdown and snap-back to Interact Or Retract passables only when the hand can currently interact with or grab them. Disable to affect all passables in that mode. Continue To Free Cell objects are always exempt.")]
    bool onlyAffectInteractablePassables;
    [SerializeField]
    [Tooltip("Snap an extended hand back to the cell before an affected passable object when the rover changes rows or columns.")]
    bool enablePassableSnapBack = true;

    [Header("Visuals")]
    [SerializeField, Min(0.01f)]
    [Tooltip("Visual retraction speed after an instant hand snap. Does not affect movement or interaction logic.")]
    float snapRetractSpeed = 60f;
    [SerializeField, Min(0f)] float visibleDistance = 0.1f;
    [SerializeField, Min(0f)] float holdingDistance = 0.5f;
    [SerializeField, Min(0f)] float emptyHandRestingDistance = 0.2f;
    [SerializeField] bool isCellHighlightEnabled;

    public float ExtendSpeed => extendSpeed;
    public float RetractSpeed => retractSpeed;
    public bool IsAutoRetractEnabled => isAutoRetractEnabled;
    public float InteractablePassThroughSpeed => interactablePassThroughSpeed;
    public bool OnlyAffectInteractablePassables => onlyAffectInteractablePassables;
    public bool EnablePassableSnapBack => enablePassableSnapBack;
    public float SnapRetractSpeed => snapRetractSpeed;
    public float VisibleDistance => visibleDistance;
    public float HoldingDistance => holdingDistance;
    public float EmptyHandRestingDistance => emptyHandRestingDistance;
    public bool IsCellHighlightEnabled => isCellHighlightEnabled;
}
