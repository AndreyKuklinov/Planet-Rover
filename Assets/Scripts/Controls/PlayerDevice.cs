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
    public event Action<PlayerDevice> DeviceLost;

    public HashSet<Direction> AllowedDirections;

    [SerializeField] PlayerInput playerInput;

    public void OnUp(InputAction.CallbackContext ctx)
    {
        HandleDirection(ctx, Direction.Up);
    }

    public void OnLeft(InputAction.CallbackContext ctx)
    {
        HandleDirection(ctx, Direction.Left);
    }

    public void OnRight(InputAction.CallbackContext ctx)
    {
        HandleDirection(ctx, Direction.Right);
    }

    public void OnDown(InputAction.CallbackContext ctx)
    {
        HandleDirection(ctx, Direction.Down);
    }

    public void OnDrop(InputAction.CallbackContext ctx)
    {
        DropTriggered?.Invoke(this);
    }

    public void OnDeviceLost(PlayerInput _playerInput)
    {
        DeviceLost?.Invoke(this);
    }

    public void OnQuit()
    {

    }

    public void OnRestart()
    {

    }

    public void OnDeleteSaveData()
    {

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