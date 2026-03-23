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

public enum InputInteraction
{
    Hold,
    Release
}

public enum HandState
{
    None,
    Extending,
    Retracting
}

public enum Signal
{
    Purple,
    Blue,
    Green,
    Yellow,
    Orange,
    Red,
    Brown
}

public static class SignalColor
{
    readonly static Color[] colors = new Color[]
    {
        ColorExtensions.FromHex("#9656a2"),
        ColorExtensions.FromHex("#369acc"),
        ColorExtensions.FromHex("#95cf92"),
        ColorExtensions.FromHex("#f8e16f"),
        ColorExtensions.FromHex("#f4895f"),
        ColorExtensions.FromHex("#de324c"),
        ColorExtensions.FromHex("#6c584c")
    };

    public static Color GetColor(Signal signal)
        => colors[(int)signal];
}

public static class DirectionVector
{
    public static Vector3 GetVector3(Direction direction) => direction switch
    {
        Direction.Up => Vector3.up,
        Direction.Down => Vector3.down,
        Direction.Left => Vector3.left,
        Direction.Right => Vector3.right,
        _ => throw new ArgumentException(direction.ToString() + " has no direction vector"),
    };

    public static Vector2Int GetVector2Int(Direction direction) => direction switch
    {
        Direction.Up => Vector2Int.up,
        Direction.Down => Vector2Int.down,
        Direction.Left => Vector2Int.left,
        Direction.Right => Vector2Int.right,
        _ => throw new ArgumentException(direction.ToString() + " has no direction vector"),
    };
}