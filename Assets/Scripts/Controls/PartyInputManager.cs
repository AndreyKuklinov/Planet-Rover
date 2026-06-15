using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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

    void OnPlayerJoined(PlayerInput playerInput)
    {
        var device = playerInput.GetComponent<PlayerDevice>();
        device.transform.SetParent(transform);
        players.Add(device);
        ConfigurePlayers();
        device.DirectionTriggered += OnDirectionTriggered;
        device.DropTriggered += OnDropTriggered;
    }

    private void OnDeviceLost(PlayerDevice player)
    {
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
        Debug.Log("player left");
    }

    void ConfigurePlayers()
    {
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
