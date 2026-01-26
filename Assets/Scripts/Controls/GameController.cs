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
