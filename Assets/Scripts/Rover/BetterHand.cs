using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BetterHand : MonoBehaviour
{
    public static event Action<BetterHand, Vector3> SelectedMovementTarget;
    [field: SerializeField] public Direction Direction { get; private set; }

    public LevelObject HeldObject { get; private set; }
    public float CurrentDistance { get; private set; } = 0;
    public bool IsExtending { get; private set; } = false;
    public bool IsRetracting { get; private set; } = false;
    
    [SerializeField] float extendSpeed;
    [SerializeField] float retractSpeed;
    [SerializeField] float holdingDistance;

    private LevelGrid levelGrid;

    public bool IsHoldingObject
        => HeldObject != null;

    public float RestingDistance
        => IsHoldingObject ? holdingDistance : 0;

    public Vector3 HandPosition
        => transform.position + CurrentDistance * DirectionVector.GetVector3(Direction);

    public void TryExtend()
    {
        if (IsExtending || IsRetracting)
            return;

        IsExtending = true;
    }

    public void TryGrab()
    {
        if (IsRetracting || !IsExtending)
            return;

        IsExtending = false;

        if (true)
        {
            SelectedMovementTarget?.Invoke(this, HandPosition);
            RetractInstantly();
        }
        else
        {
            IsRetracting = true;
        }
    }

    void Start()
    {
        levelGrid = FindObjectOfType<LevelGrid>();
    }

    void Update()
    {
        UpdateExtension();
        UpdateRetraction();
    }

    void RetractInstantly()
    {
        CurrentDistance = RestingDistance;
    }

    void UpdateExtension()
    {
        if (!IsExtending)
            return;

        CurrentDistance += extendSpeed * Time.deltaTime;

        var cell = levelGrid.WorldToCell(transform.position);
        var stoppingCell = levelGrid.GetStoppingSquare(cell, Direction, HeldObject);
        var maxDist = (levelGrid.CellToWorld(stoppingCell) - transform.position).magnitude;

        CurrentDistance = Mathf.Min(CurrentDistance, maxDist);
    }

    void UpdateRetraction()
    {
        if (!IsRetracting)
            return;

        CurrentDistance -= retractSpeed * Time.deltaTime;
        CurrentDistance = Mathf.Max(CurrentDistance, RestingDistance);

        if (CurrentDistance <= RestingDistance)
            IsRetracting = false;
    }
}
