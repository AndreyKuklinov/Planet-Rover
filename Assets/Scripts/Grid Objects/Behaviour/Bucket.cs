using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IFillable
{
    [SerializeField] FilledContainerSpriteRepo containerFillRepo;
    [SerializeField] ItemContainerData containerData;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Deliverable deliverable;
    [SerializeField] DeliverableData bucketDeliverableData;

    public ContainableData CurrentContainedData { get; private set; }

    public bool CanContain(ContainableData data)
    {
        return data.IsLiquid;
    }

    public void Empty()
    {
        CurrentContainedData = null;
        spriteRenderer.sprite = containerData.EmptySprite;
        deliverable.DeliverableData = bucketDeliverableData;
    }

    public void FillWith(ContainableData containable)
    {
        CurrentContainedData = containable;
        var sprite = containerFillRepo.GetSprite(containerData, containable);
        spriteRenderer.sprite = sprite;
        deliverable.DeliverableData = data.DeliverableData;
    }
}