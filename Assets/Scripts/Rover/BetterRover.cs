using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BetterRover : MonoBehaviour
{
    public bool IsMoving { get; private set; }
    public Vector3 TargetPosition { get; private set; }
    public BetterHand TargetHand { get; private set; }

    [SerializeField] BetterHand[] hands;
    [SerializeField] float movementSpeed;

    private LevelGrid levelGrid;

    public void TryExtend(Direction direction)
    {
        hands[(int)direction].TryExtend();
    }

    public void TryGrab(Direction direction)
    {
        hands[(int)direction].TryGrab();
    }

    void Start()
    {
        levelGrid = FindObjectOfType<LevelGrid>();
        BetterHand.SelectedMovementTarget += OnSelectedMovementTarget;
    }

    void Update()
    {
        MoveToTarget();
        CheckIfReachedTarget();
    }

    void OnSelectedMovementTarget(BetterHand hand, Vector3 target)
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
