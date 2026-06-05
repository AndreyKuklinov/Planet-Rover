using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rover : MonoBehaviour, IPassable
{
    public RoverHand TargetHand { get; private set; }

    [SerializeField] RoverHand[] hands;
    [SerializeField] GridObject gridObject;
    [SerializeField] GridObjectMover mover;
    [SerializeField] bool isRetractionVoluntary;

    public RoomGrid LevelGrid
        => gridObject.RoomGrid;

    public bool IsMoving
        => mover.IsMoving;

    public void OnPress(Direction direction)
    {
        hands[(int)direction].TryExtend();
    }

    public void OnRelease(Direction direction)
    {
        if(isRetractionVoluntary)
            hands[(int)direction].TryInteract();
    }

    public void OnDrop(IEnumerable<Direction> direction)
    {
        foreach(var dir in direction)
            hands[(int)dir].TryDrop();
    }

    void Start()
    {
        RoverHand.SelectedMovementTarget += OnSelectedMovementTarget;
    }

    void OnDestroy()
    {
        RoverHand.SelectedMovementTarget -= OnSelectedMovementTarget;
    }

    void OnSelectedMovementTarget(RoverHand hand, Vector3 target)
    {
        TargetHand = hand;
        var cell = LevelGrid.WorldToCell(target);
        var pos = LevelGrid.CellToWorld(cell);

        if (!LevelGrid.IsWithinBounds(cell))
            return;

        mover.MoveToPosition(pos);
    }   
}
