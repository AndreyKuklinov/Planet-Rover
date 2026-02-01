using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
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
    public event Action<PlayerDevice> DropTriggered;

    public HashSet<Direction> AllowedDirections;

    [SerializeField] PlayerInput playerInput;

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
            case "Drop":
                DropTriggered?.Invoke(this);
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

        DirectionTriggered?.Invoke(direction, InputInteraction.Hold, this);
    }
}