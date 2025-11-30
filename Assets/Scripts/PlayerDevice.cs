using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDevice : MonoBehaviour
{
    public HashSet<Directions> AllowedDirections = new HashSet<Directions>() { Directions.Right, Directions.Left };
    public Rover Rover;

    [SerializeField] bool isTestingOn;

    void OnUp(InputValue value)
    {
        OnInput(value, Directions.Up);
    }

    void OnLeft(InputValue value)
    {
        OnInput(value, Directions.Left);
    }

    void OnDown(InputValue value)
    {
        OnInput(value, Directions.Down);
    }

    void OnRight(InputValue value)
    {
        OnInput(value, Directions.Right);
    }

    void OnInput(InputValue value, Directions direction)
    {
        if (!isTestingOn && !AllowedDirections.Contains(direction))
            return;

        if (value.isPressed)
            Rover.OnArmPressed(direction);
        else
            Rover.OnArmReleased(direction);
    }
}
