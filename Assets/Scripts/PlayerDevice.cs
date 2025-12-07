using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDevice : MonoBehaviour
{
    public HashSet<Direction> AllowedDirections;
    private GameController gameController;

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
        if(gameController == null)
            gameController = FindObjectOfType<GameController>();

        gameController.PressDirection(value, direction, this);
    }
}
