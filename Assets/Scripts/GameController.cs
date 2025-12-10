using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] ArmManager armManager;
    [SerializeField] PartyInputManager partyManager;

    void Start()
    {
        partyManager.DirectionTriggered += OnDirectionTriggered;
    }

    void OnDirectionTriggered(InputValue value, Direction direction, PlayerDevice player)
    {
        if (!player.AllowedDirections.Contains(direction))
            return;
        if (value.isPressed)
            armManager.Extend(direction);
        else
            armManager.Grab(direction);
    }
}
