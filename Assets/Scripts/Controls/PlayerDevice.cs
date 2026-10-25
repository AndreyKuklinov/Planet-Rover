using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Controls;
using System.Linq;

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
    int lastDropFrame = -1;
    public enum KeyboardGroup { None, Wasd, Arrows }
    public KeyboardGroup Keys { get; private set; }
    public Keyboard SharedKeyboard => playerInput == null ? null : playerInput.devices.OfType<Keyboard>().FirstOrDefault();
    public bool IsDropHeld => isActiveAndEnabled && playerInput != null && playerInput.isActiveAndEnabled
        && DropButtons().Any(button => button.isPressed);

    IEnumerable<ButtonControl> DropButtons()
    {
        if (playerInput == null || !playerInput.inputIsActive || playerInput.actions == null) yield break;
        foreach (var device in playerInput.devices)
        {
            IEnumerable<ButtonControl> buttons = device is Keyboard keyboard
                ? keyboard.allKeys.Cast<ButtonControl>() : device.allControls.OfType<ButtonControl>();
            foreach (var button in buttons)
            {
                if (button.synthetic) continue;
                if (device is Keyboard && IsOtherKeyboardGroup(button.name)) continue;
                if (playerInput.actions.Any(action => ReservesButton(action) && action.controls.Any(control => control == button))) continue;
                yield return button;
            }
        }
    }

    bool ReservesButton(InputAction action)
    {
        if (action.name == "Drop") return false;
        if (Enum.TryParse<Direction>(action.name, out var direction))
            return AllowedDirections == null || AllowedDirections.Contains(direction);
        return true;
    }

    bool IsOtherKeyboardGroup(string key)
    {
        bool wasd = key == "w" || key == "a" || key == "s" || key == "d";
        bool arrows = key == "upArrow" || key == "downArrow" || key == "leftArrow" || key == "rightArrow";
        return (Keys == KeyboardGroup.Wasd && arrows) || (Keys == KeyboardGroup.Arrows && wasd);
    }

    public void SetKeyboardGroup(KeyboardGroup group)
    {
        Keys = group;
        foreach (var action in playerInput.actions)
        {
            if (action.name != "Up" && action.name != "Down" && action.name != "Left" && action.name != "Right") continue;
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var path = action.bindings[i].path;
                if (!path.StartsWith("<Keyboard>/")) continue;
                bool arrow = path.Contains("Arrow");
                if (arrow == (group == KeyboardGroup.Arrows)) action.RemoveBindingOverride(i);
                else action.ApplyBindingOverride(i, "");
            }
        }
    }

    void Update()
    {
        if (DropButtons().Any(button => button.wasPressedThisFrame)) TriggerDrop();

        if (playerInput == null || !playerInput.isActiveAndEnabled || !playerInput.inputIsActive
            || playerInput.actions == null) return;
        // Retry extension while held, including after the hand finishes retracting.
        // Poll the player's actions so keyboard profiles and gamepad bindings stay respected.
        foreach (var action in playerInput.actions)
            if (action.IsPressed() && Enum.TryParse<Direction>(action.name, out var direction)
                && (AllowedDirections == null || AllowedDirections.Contains(direction)))
                DirectionTriggered?.Invoke(direction, InputInteraction.Hold, this);
    }

    void TriggerDrop()
    {
        if (lastDropFrame == Time.frameCount) return;
        lastDropFrame = Time.frameCount;
        DropTriggered?.Invoke(this);
    }

    [SerializeField] PlayerInput playerInput;
    [SerializeField] VoidEventChannel quitTriggered;
    [SerializeField] VoidEventChannel restartTriggered;
    [SerializeField] VoidEventChannel deleteSaveDataTriggered;

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
        // Drop is polled from unused buttons, including shared keyboard keys.
    }

    public void OnDeviceLost(PlayerInput _playerInput)
    {
        DeviceLost?.Invoke(this);
    }

    public void OnQuit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !CanPerformSharedAction())
            return;
        quitTriggered.Raise();
    }

    public void OnRestart(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !CanPerformSharedAction())
            return;
        restartTriggered.Raise();
    }

    public void OnDelete(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || !CanPerformSharedAction())
            return;
        deleteSaveDataTriggered.Raise();
    }

    void HandleDirection(InputAction.CallbackContext ctx, Direction direction)
    {
        if (AllowedDirections != null && !AllowedDirections.Contains(direction))
        {
            if (ctx.performed) TriggerDrop();
            return;
        }

        if(ctx.canceled)
        {
            DirectionTriggered?.Invoke(direction, InputInteraction.Release, this);
            return;
        }

        DirectionTriggered?.Invoke(direction, InputInteraction.Hold, this);
    }

    bool CanPerformSharedAction() => SharedKeyboard == null || PartyInputManager.Instance == null
        || PartyInputManager.Instance.IsPrimaryKeyboardPlayer(this);
}
