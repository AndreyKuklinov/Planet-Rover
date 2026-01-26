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

    public void TryExtend(Direction direction)
    {
        hands[(int)direction].TryExtend();
    }

    public void TryGrab(Direction direction)
    {
        hands[(int)direction].TryInteract();
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
            OnFinishedMoving();
        }
    }

    void OnFinishedMoving()
    {
        TargetHand.TryGrab();
    }
}
