using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        rover.OnDrop(obj.AllowedDirections);
    }

    void OnDirectionTriggered(Direction direction, InputInteraction interaction, PlayerDevice player)
    {
        if (!player.AllowedDirections.Contains(direction) && !isTestingModeOn)
            return;

        switch (interaction)
        {
            case InputInteraction.Hold:
                rover.OnPress(direction);
                break;
            case InputInteraction.Release:
                rover.OnRelease(direction);
                break;
        }
    }
}
