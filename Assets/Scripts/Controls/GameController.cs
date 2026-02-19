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

    void Start()
    {
        partyManager.DirectionTriggered += OnDirectionTriggered;
        partyManager.DropTriggered += OnDropTriggered;
    }

    void OnDropTriggered(PlayerDevice obj)
    {
        if (Rover.Current == null)
            return;

        if (gameManager.IsGameOver)
            return;

        Rover.Current.OnDrop(obj.AllowedDirections);
    }

    void OnDirectionTriggered(Direction direction, InputInteraction interaction, PlayerDevice player)
    {
        if (Rover.Current == null)
            return;

        if (gameManager.IsGameOver)
            return;

        if (!player.AllowedDirections.Contains(direction) && !isTestingModeOn)
            return;

        switch (interaction)
        {
            case InputInteraction.Hold:
                Rover.Current.OnPress(direction);
                break;
            case InputInteraction.Release:
                Rover.Current.OnRelease(direction);
                break;
        }
    }
}
