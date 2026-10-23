using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PartyInputManager : MonoBehaviour
{
    public static PartyInputManager Instance { get; private set; }

    public event PlayerDevice.DirectionTriggeredHandler DirectionTriggered;
    public event Action<PlayerDevice> DropTriggered;

    public readonly Dictionary<Direction, int> DirectionToPlayerIndex = new()
    {
        { Direction.Up, 0 },
        { Direction.Down, 0 },
        { Direction.Left, 0 },
        { Direction.Right, 0 },
    };

    private List<PlayerDevice> players = new();
    public int PlayerCount => players.Count;
    public int RosterVersion { get; private set; }
    public bool AllPlayersHoldingDrop => players.Count > 0 && players.All(p => p != null && p.IsDropHeld);
    public bool AnyPlayerHoldingDrop => players.Any(p => p != null && p.IsDropHeld);

    private readonly Dictionary<int, HashSet<Direction>[]> allowedDirections = new()
    {
        {
            1,
            new[]
            {
                new HashSet<Direction>() { Direction.Left, Direction.Right, Direction.Up, Direction.Down }
            }
        },
        {
            2,
            new[]
            {
                new HashSet<Direction>() { Direction.Up, Direction.Down },
                new HashSet<Direction>() { Direction.Left, Direction.Right },
            }
        },
        {
            3,
            new[]
            {
                new HashSet<Direction>() { Direction.Up, Direction.Down },
                new HashSet<Direction>() { Direction.Left },
                new HashSet<Direction>() { Direction.Right },
            }
        },
        {
            4,
            new[]
            {
                new HashSet<Direction>() { Direction.Up },
                new HashSet<Direction>() { Direction.Down },
                new HashSet<Direction>() { Direction.Left },
                new HashSet<Direction>() { Direction.Right },
            }
        },
    };

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        else
            Destroy(gameObject);
    }

    void Start()
    {
        var manager = GetComponent<PlayerInputManager>();
        if (manager != null) manager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
    }

    void Update()
    {
        var manager = GetComponent<PlayerInputManager>();
        if (manager == null || !manager.joiningEnabled || players.Count >= 4) return;
        foreach (var keyboard in InputSystem.devices.OfType<Keyboard>())
        {
            TryJoinKeyboard(manager, keyboard, PlayerDevice.KeyboardGroup.Wasd,
                keyboard.wKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame);
            TryJoinKeyboard(manager, keyboard, PlayerDevice.KeyboardGroup.Arrows,
                keyboard.upArrowKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame);
        }
        foreach (var gamepad in Gamepad.all)
            if (players.Count < 4 && !players.Any(p => p.GetComponent<PlayerInput>().devices.Contains(gamepad))
                && gamepad.allControls.OfType<ButtonControl>().Any(b => !b.synthetic && b.wasPressedThisFrame))
                manager.JoinPlayer(controlScheme: "Gamepad", pairWithDevice: gamepad);
    }

    void TryJoinKeyboard(PlayerInputManager manager, Keyboard keyboard, PlayerDevice.KeyboardGroup group, bool pressed)
    {
        if (!pressed || players.Count >= 4 || players.Any(p => p.SharedKeyboard == keyboard && p.Keys == group)) return;
        var input = manager.JoinPlayer(controlScheme: "Keyboard", pairWithDevice: keyboard);
        if (input != null) input.GetComponent<PlayerDevice>().SetKeyboardGroup(group);
    }

    public bool IsPrimaryKeyboardPlayer(PlayerDevice player)
        => players.FirstOrDefault(p => p.SharedKeyboard == player.SharedKeyboard) == player;

    void OnPlayerJoined(PlayerInput playerInput)
    {
        playerInput.neverAutoSwitchControlSchemes = true;
        var device = playerInput.GetComponent<PlayerDevice>();
        device.transform.SetParent(transform);
        players.Add(device);
        ConfigurePlayers();
        device.DirectionTriggered += OnDirectionTriggered;
        device.DropTriggered += OnDropTriggered;
        device.DeviceLost += OnDeviceLost;
    }

    private void OnDeviceLost(PlayerDevice player)
    {
        if (!players.Contains(player)) return;
        player.DirectionTriggered -= OnDirectionTriggered;
        player.DropTriggered -= OnDropTriggered;
        player.DeviceLost -= OnDeviceLost;
        players.Remove(player);
        Destroy(player.gameObject);
        ConfigurePlayers();
    }

    private void OnDropTriggered(PlayerDevice obj)
    {
        DropTriggered?.Invoke(obj);
    }

    void OnDirectionTriggered(Direction direction, InputInteraction interaction, PlayerDevice playerDevice)
    {
        DirectionTriggered?.Invoke(direction, interaction, playerDevice);
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        OnDeviceLost(playerInput.GetComponent<PlayerDevice>());
    }

    void ConfigurePlayers()
    {
        RosterVersion++;
        for (var i = 0; i < players.Count; i++)
        {
            players[i].AllowedDirections = allowedDirections[players.Count][i];
            foreach (var direction in players[i].AllowedDirections)
            {
                DirectionToPlayerIndex[direction] = i;
            }
        }
    }
}
