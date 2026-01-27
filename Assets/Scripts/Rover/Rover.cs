using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rover : MonoBehaviour
{
    public bool IsMoving { get; private set; }
    public Vector3 TargetPosition { get; private set; }
    public RoverHand TargetHand { get; private set; }

    [SerializeField] RoverHand[] hands;
    [SerializeField] float movementSpeed;

    private LevelGrid levelGrid;
    private RoverHand lastPressedHand;

    public void OnPress(Direction direction)
    {
        var hand = hands[(int)direction];

        if (lastPressedHand != null && lastPressedHand != hand && lastPressedHand.IsHoldingObject)
            lastPressedHand.PassObject(hand);

        hand.TryExtend();
        lastPressedHand = hand;
    }

    public void OnRelease(Direction direction)
    {
        hands[(int)direction].TryInteract();
    }

    public void OnDoubleTap(Direction direction)
    {
        hands[(int)direction].TryDrop();
    }

    void Start()
    {
        levelGrid = FindObjectOfType<LevelGrid>();
        RoverHand.SelectedMovementTarget += OnSelectedMovementTarget;
    }

    void Update()
    {
        MoveToTarget();
        CheckIfReachedTarget();
    }

    void OnSelectedMovementTarget(RoverHand hand, Vector3 target)
    {
        TargetHand = hand;
        TargetPosition = levelGrid.SnapToGrid(target);
        IsMoving = true;
    }

    void MoveToTarget()
    {
        if (!IsMoving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetPosition,
            movementSpeed * Time.deltaTime
        );
    }

    void CheckIfReachedTarget()
    {
        if (!IsMoving)
            return;

        if (Vector3.Distance(transform.position, TargetPosition) <= 0.001f)
        {
            IsMoving = false;
        }
    }
}
