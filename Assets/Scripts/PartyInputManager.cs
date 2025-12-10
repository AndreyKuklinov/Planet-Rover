using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PartyInputManager : MonoBehaviour
{
    public event PlayerDevice.DirectionTriggeredHandler DirectionTriggered;

    private readonly List<PlayerDevice> players = new();

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
        }
    };

    void OnPlayerJoined(PlayerInput playerInput)
    {
        var device = playerInput.GetComponent<PlayerDevice>();
        players.Add(device);
        ConfigurePlayers();
        device.DirectionTriggered += OnDirectionTriggered;
    }

    void OnDirectionTriggered(InputValue inputValue, Direction direction, PlayerDevice playerDevice)
    {
        DirectionTriggered?.Invoke(inputValue, direction, playerDevice);
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        players.Remove(playerInput.GetComponent<PlayerDevice>());
        ConfigurePlayers();
    }

    void ConfigurePlayers()
    {
        for (var i = 0; i < players.Count; i++)
        {
            players[i].AllowedDirections = allowedDirections[players.Count][i];
        }
    }
}
