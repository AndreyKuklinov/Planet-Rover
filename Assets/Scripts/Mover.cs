using System;
using UnityEngine;

public class Mover : MonoBehaviour
{
    // TODO: Possibly reorganise this functional nonsense
    public event Action ReachedDestination;

    public bool IsMoving { get; private set; }

    private Action move = () => { };
    private Vector2 currentVector;
    private float movementSpeed;

    void Update()
    {
        move();
    }

    public void MoveInDirection(Direction direction, float speed)
    {
        var v = direction switch
        {
            Direction.Up => Vector2.up,
            Direction.Down => Vector2.down,
            Direction.Left => Vector2.left,
            Direction.Right => Vector2.right,
            _ => throw new ArgumentException("Can't move in direction " + direction),
        };
        currentVector = v;
        move = HandleMovementInDirection;
        movementSpeed = speed;
        IsMoving = true;
    }

    public void MoveToPosition(Vector2 position, float speed)
    {
        currentVector = position;
        move = HandleMovementToPosition;
        movementSpeed = speed;
        IsMoving = true;
    }

    public void StopMoving()
    {
        move = () => { };
        IsMoving = false;
    }

    void HandleMovementInDirection()
    {
        transform.position += (Vector3)(movementSpeed * Time.deltaTime * currentVector);
        return;
    }

    void HandleMovementToPosition()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentVector,
            movementSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, currentVector) <= 0.001f)
            ReachedDestination?.Invoke();
    }
}