using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Mover : MonoBehaviour
{
    public bool IsMoving { get; private set; }

    private IMover mover;

    public bool IsAtDestination
        => mover != null && mover.IsAtDestination;

    void Update()
    {
        if (IsMoving)
            mover.Move();
    }

    public void MoveInDirection(Direction direction, float speed)
    {
        mover = new MoverInDirection(transform, direction, speed);
        IsMoving = true;
    }

    public void MoveToTransform(Transform target, float speed)
    {
        mover = new MoverToTransform(transform, target, speed);
        IsMoving = true;
    }

    public void MoveToPosition(Vector3 position, float speed)
    {
        mover = new MoverToPosition(transform, position, speed);
        IsMoving = true;
    }

    public void StopMoving()
    {
        IsMoving = false;
    }

    interface IMover
    {
        public void Move();

        public bool IsAtDestination { get; }
    }

    class MoverInDirection : IMover
    {
        private readonly Dictionary<Direction, Vector2> directionToVector = new()
        {
            { Direction.Up, Vector2.up },
            { Direction.Down, Vector2.down },
            { Direction.Left, Vector2.left },
            { Direction.Right, Vector2.right },
        };

        private readonly Vector2 movementVector;
        private readonly Transform obj;
        private readonly float movementSpeed;

        public MoverInDirection(Transform obj, Direction direction, float movementSpeed)
        {
            movementVector = directionToVector[direction];
            this.obj = obj;
            this.movementSpeed = movementSpeed;
        }

        public void Move()
        {
            obj.position += (Vector3)(movementSpeed * Time.deltaTime * movementVector);
        }

        public bool IsAtDestination
            => false;
    }

    class MoverToTransform : IMover
    {
        private Transform obj;
        private Transform target;
        private float movementSpeed;

        public MoverToTransform(Transform obj, Transform target, float movementSpeed)
        {
            this.obj = obj;
            this.target = target;
            this.movementSpeed = movementSpeed;
        }

        public void Move()
        {
            obj.position = Vector3.MoveTowards(
            obj.position,
            target.position,
            movementSpeed * Time.deltaTime
            );
        }

        public bool IsAtDestination
            => Vector3.Distance(obj.position, target.position) <= 0.001f;
    }

    class MoverToPosition : IMover
    {
        private Transform obj;
        private Vector3 target;
        private float movementSpeed;

        public MoverToPosition(Transform obj, Vector3 target, float movementSpeed)
        {
            this.obj = obj;
            this.target = target;
            this.movementSpeed = movementSpeed;
        }

        public void Move()
        {
            obj.position = Vector3.MoveTowards(
            obj.position,
            target,
            movementSpeed * Time.deltaTime
            );
        }

        public bool IsAtDestination
            => Vector3.Distance(obj.position, target) <= 0.001f;
    }
}