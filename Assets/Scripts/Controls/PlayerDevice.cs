using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDevice : MonoBehaviour
{
    public delegate void DirectionTriggeredHandler(
        InputValue inputValue, 
        Direction direction, 
        PlayerDevice playerDevice
    );
    public event DirectionTriggeredHandler DirectionTriggered;
    public event Action<PlayerDevice> DropTriggered;

    public HashSet<Direction> AllowedDirections;
    //private GameController gameController;

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

    void OnDrop()
    {
        DropTriggered?.Invoke(this);
    }

    void OnInput(InputValue value, Direction direction)
    {
        //if(gameController == null)
        //    gameController = FindObjectOfType<GameController>();

        DirectionTriggered?.Invoke(value, direction, this);
    }
}
