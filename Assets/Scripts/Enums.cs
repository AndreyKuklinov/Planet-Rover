using System;
using Unity.VisualScripting;
using UnityEngine;

public enum Direction
{
    Up = 0,
    Left = 1,
    Down = 2,
    Right = 3,
}

public enum HandBehaviour
{
    GoThrough,
    HoverOver,
    StopBefore,
    Retract
}

public static class DirectionVector
{
    public static Vector3 Get(Direction direction) => direction switch
    {
        Direction.Up => Vector3.up,
        Direction.Down => Vector3.down,
        Direction.Left => Vector3.left,
        Direction.Right => Vector3.right,
        _ => throw new ArgumentException(direction.ToString() + " has no direction vector"),
    };
}