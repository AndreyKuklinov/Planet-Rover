using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerDevice : MonoBehaviour
{
    public delegate void DirectionTriggeredHandler(
        Direction direction,
        InputInteraction interaction,
        PlayerDevice playerDevice
    );

    public event DirectionTriggeredHandler DirectionTriggered;
    public HashSet<Direction> AllowedDirections;

    [SerializeField] PlayerInput playerInput;
    [SerializeField] float doubleTapTime;

    Dictionary<Direction, float> directionTapTimes = new();

    void Start()
    {
        playerInput.onActionTriggered += OnActionTriggered;
    }

    void OnActionTriggered(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed && !ctx.canceled)
            return;

        switch (ctx.action.name)
        {
            case "Up":
                HandleDirection(ctx, Direction.Up);
                break;
            case "Down":
                HandleDirection(ctx, Direction.Down);
                break;
            case "Left":
                HandleDirection(ctx, Direction.Left);
                break;
            case "Right":
                HandleDirection(ctx, Direction.Right);
                break;
        }
    }

    void HandleDirection(InputAction.CallbackContext ctx, Direction direction)
    {
        if(ctx.canceled)
        {
            DirectionTriggered?.Invoke(direction, InputInteraction.Release, this);
            return;
        }

        if (!ctx.performed)
            return;

        var timeSinceLastTap = Time.time - directionTapTimes.GetValueOrDefault(direction);
        directionTapTimes[direction] = Time.time;

        if (timeSinceLastTap <= doubleTapTime)
        {
            DirectionTriggered?.Invoke(direction, InputInteraction.DoubleTap, this);
            return;
        }

        DirectionTriggered?.Invoke(direction, InputInteraction.Hold, this);
    }
}