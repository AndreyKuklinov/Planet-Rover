using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomExit : MonoBehaviour, IInteractable
{
    [SerializeField] VoidEventChannel roomExitTriggered;
    [SerializeField] Objective objective;
    [SerializeField] ObjectiveTracker objectiveTracker;
    [SerializeField] Sprite disabledSprite;
    [SerializeField] Sprite enabledSprite;
    [SerializeField] SpriteRenderer spriteRenderer;

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return true;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        objective.Complete();
        roomExitTriggered.Raise();
        return grabbedObject;
    }

    void Update()
    {
        var unfulfilledCount = objectiveTracker.AllObjectives.Count 
            - objectiveTracker.FulfilledObjectives.Count;

        spriteRenderer.sprite = unfulfilledCount > 1 ? disabledSprite : enabledSprite;
    }
}
