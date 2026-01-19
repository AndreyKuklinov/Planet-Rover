using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] bool isTestingModeOn = false;
    [SerializeField] Rover armManager;
    [SerializeField] PartyInputManager partyManager;

    void Start()
    {
        partyManager.DirectionTriggered += OnDirectionTriggered;
    }

    void OnDirectionTriggered(InputValue value, Direction direction, PlayerDevice player)
    {
        Debug.Log(direction.ToString() + "?");
        if (!player.AllowedDirections.Contains(direction) && !isTestingModeOn)
        {
            Debug.Log("Denied.");
            return;
        }
        if (value.isPressed)
        {
            Debug.Log("Pressed.");
            armManager.Extend(direction);
        }
        else
        {
            Debug.Log("Not pressed.");
            armManager.Grab(direction);
        }
    }
}
