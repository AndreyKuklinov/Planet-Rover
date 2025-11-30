using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PartyInputManager : MonoBehaviour
{
    [SerializeField] Rover rover;

    private List<PlayerController> players = new List<PlayerController>();

    private readonly Dictionary<int, HashSet<Direction>[]> allowedDirections = new Dictionary<int, HashSet<Direction>[]>()
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
        players.Add(playerInput.GetComponent<PlayerController>());
        ConfigurePlayers();
    }

    void OnPlayerLeft(PlayerInput playerInput)
    {
        players.Remove(playerInput.GetComponent<PlayerController>());
        ConfigurePlayers();
    }

    void ConfigurePlayers()
    {
        for (var i = 0; i < players.Count; i++)
        {
            players[i].Rover = rover;
            players[i].AllowedDirections = allowedDirections[players.Count][i];
        }
    }
}
