using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableAggregator : MonoBehaviour, IInteractable
{
    private List<IInteractable> interactions = new List<IInteractable>();

    private void Awake()
    {
        IInteractable[] allInteractables = GetComponentsInChildren<IInteractable>();
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
        interactions = allInteractables.Where(x => x != this).ToList();
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast
        if (interactions.Count == 0)
        {
            Debug.LogWarning($"InteractableAggregator on {gameObject.name} found no interactions!", this);
        }
    }

    public bool CanInteractWith(IGrabbable grabbedObject)
    {
        return GetValidInteraction(grabbedObject) != null;
    }

    public IGrabbable InteractWith(IGrabbable grabbedObject)
    {
        IInteractable targetInteraction = GetValidInteraction(grabbedObject);

        if (targetInteraction == null)
        {
            throw new InvalidOperationException("Trying to interact with an object that can't interact with: "
                + this.gameObject.name + ", " + grabbedObject.GridObject);
        }

        return targetInteraction.InteractWith(grabbedObject);
    }

    private IInteractable GetValidInteraction(IGrabbable grabbedObject)
    {
        IInteractable validInteraction = null;

        foreach (var interaction in interactions)
        {
            if (interaction.CanInteractWith(grabbedObject))
            {
                if (validInteraction != null)
                {
                    throw new Exception("Multiple interactions are possible at the same time with a grid object: "
                        + this.gameObject.name);
                }

                validInteraction = interaction;
            }
        }

        return validInteraction;
    }
}