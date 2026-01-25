using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BetterHand : MonoBehaviour
{
    [field: SerializeField] public Direction Direction { get; private set; }

    public LevelObject HeldObject { get; private set; }
    public float CurrentDistance { get; private set; } = 0;
    public bool IsExtending { get; private set; } = false;
    public bool IsRetracting { get; private set; } = false;
    
    [SerializeField] float extendSpeed;
    [SerializeField] float retractSpeed;

    private LevelGrid levelGrid;

    public bool IsHoldingObject
        => HeldObject != null;

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
        if (IsRetracting)
            return;
    }

    void Start()
    {
        levelGrid = FindObjectOfType<LevelGrid>();
    }

    void Update()
    {
        UpdateExtension();
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

        Debug.Log(CurrentDistance);
    }

}
