using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Rover : MonoBehaviour
{
    public RoverHand TargetHand { get; private set; }

    [SerializeField] RoverHand[] hands;
    [SerializeField] LevelObject levelObject;
    [SerializeField] bool isRetractionVoluntary;

    public LevelGrid LevelGrid
        => levelObject.LevelGrid;

    public bool IsMoving
        => levelObject.IsMoving;

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

        levelObject.MoveToPosition(pos);
    }   
}
