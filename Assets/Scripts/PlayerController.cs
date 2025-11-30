using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public HashSet<Direction> AllowedDirections;
    public Rover Rover;

    [SerializeField] bool isTestingOn;

    void OnUp(InputValue value)
    {
        OnInput(value, Direction.Up);
    }

    void OnLeft(InputValue value)
    {
        OnInput(value, Direction.Left);
    }

    void OnDown(InputValue value)
    {
        OnInput(value, Direction.Down);
    }

    void OnRight(InputValue value)
    {
        OnInput(value, Direction.Right);
    }

    void OnInput(InputValue value, Direction direction)
    {
        if (!isTestingOn && !AllowedDirections.Contains(direction))
            return;

        if (value.isPressed)
            Rover.OnArmPressed(direction);
        else
            Rover.OnArmReleased(direction);
    }
}
