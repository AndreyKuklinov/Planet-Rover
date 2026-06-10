using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObjectMover : MonoBehaviour
{
    private const float REQUIRED_DISTANCE_FROM_TARGET = 0.001f;

    [SerializeField] float movementSpeed;
    [SerializeField] GridObject gridObject;

    private Vector2 destination;

    public bool IsMoving { get; private set; }

    public void MoveToPosition(Vector2 destination) 
    {
        gridObject.TryRemoveFromGrid();

        this.destination = destination;
        IsMoving = true;
    }

    void Update()
    {
        MoveToTarget();
        CheckIfReachedTarget();
    }

    void MoveToTarget()
    {
        if (!IsMoving)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            movementSpeed * Time.deltaTime
        );
    }

    void CheckIfReachedTarget()
    {
        if (!IsMoving)
            return;

        if (Vector3.Distance(transform.position, destination) <= REQUIRED_DISTANCE_FROM_TARGET)
        {
            IsMoving = false;
            gridObject.AttachToGrid();
        }
    }
}
