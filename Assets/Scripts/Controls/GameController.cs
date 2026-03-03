using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    [SerializeField] bool isTestingModeOn = false;
    [SerializeField] PartyInputManager partyManager;
    [SerializeField] GameManager gameManager;
    [SerializeField] Rover rover;

    public bool CanControl
        => TryFindRover();

    void Start()
    {
        partyManager.DirectionTriggered += OnDirectionTriggered;
        partyManager.DropTriggered += OnDropTriggered;
    }

    void OnDropTriggered(PlayerDevice obj)
    {
        if (!CanControl)
            return;

        rover.OnDrop(obj.AllowedDirections);
    }

    void OnDirectionTriggered(Direction direction, InputInteraction interaction, PlayerDevice player)
    {
        if (!CanControl)
            return;

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

    bool TryFindRover()
    {
        if (rover != null)
            return true;

        rover = FindObjectOfType<Rover>();

        if (rover == null)
            return false;

        return true;
    }
}
