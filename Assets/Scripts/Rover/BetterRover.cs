using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BetterRover : MonoBehaviour
{
    [SerializeField] BetterHand[] hands;
    [SerializeField] float movementSpeed;

    private LevelGrid levelGrid;
    private Vector3 targetPosition;
    private bool isMoving;

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
        BetterHand.SelectedMovementCell += OnSelectedMovementCell;
    }

    void Update()
    {
        MoveToTarget();
        CheckIfReachedTarget();
    }

    void OnSelectedMovementCell(Vector2Int cell)
    {
        StartMovingToCell(cell);
    }

    void StartMovingToCell(Vector2Int cell)
    {
        targetPosition = levelGrid.CellToWorld(cell);
        isMoving = true;
    }

    void MoveToTarget()
    {
        if (!isMoving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            movementSpeed * Time.deltaTime
        );
    }

    void CheckIfReachedTarget()
    {
        if (!isMoving)
            return;

        if(Vector3.Distance(transform.position, targetPosition) <= 0.001f)
            isMoving = false;
    }
}
