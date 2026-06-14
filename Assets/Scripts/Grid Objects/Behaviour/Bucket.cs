using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IFillable
{
    [SerializeField] ContainerFillRepo containerFillRepo;
    [SerializeField] ItemContainerData containerData;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Deliverable deliverable;

    public ContainableData CurrentContainedData { get; private set; }

    public bool CanContain(ContainableData data)
    {
        return data.IsLiquid;
    }

    public void Empty()
    {
        CurrentContainedData = null;
        spriteRenderer.sprite = containerData.EmptySprite;
        deliverable.DeliverableData = null;
    }

    public void FillWith(ContainableData containable)
    {
        CurrentContainedData = containable;
        var data = containerFillRepo.GetFillData(containerData, containable);
        spriteRenderer.sprite = data.FilledSprite;
        deliverable.DeliverableData = data.DeliverableData;
    }
}