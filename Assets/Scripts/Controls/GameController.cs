using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] bool isTestingModeOn = false;
    [SerializeField] Rover rover;
    [SerializeField] PartyInputManager partyManager;

    void Start()
    {
        partyManager.DirectionTriggered += OnDirectionTriggered;
        partyManager.DropTriggered += OnDropTriggered;
    }

    private void OnDropTriggered(PlayerDevice obj)
    {
        foreach(var dir in obj.AllowedDirections)
            rover.TryDrop(dir);
    }

    void OnDirectionTriggered(InputValue value, Direction direction, PlayerDevice player)
    {
        if (!player.AllowedDirections.Contains(direction) && !isTestingModeOn)
        {
            return;
        }
        if (value.isPressed)
        {
            rover.TryExtend(direction);
        }
        else
        {
            rover.TryGrab(direction);
        }
    }
}
