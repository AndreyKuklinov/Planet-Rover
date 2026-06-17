using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IDeliverable
{
    [SerializeField] FilledBucketRepo filledBucketRepo;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] DeliverableData bucketDeliverableData;
    [SerializeField] Sprite emptySprite;

    public LiquidData CurrentLiquid { get; private set; }

    public bool IsFilled
        => CurrentLiquid != null;

    public DeliverableData DeliverableData
        => IsFilled ? CurrentLiquid.DeliverableData : bucketDeliverableData;

    public void Empty()
    {
        CurrentLiquid = null;
        spriteRenderer.sprite = emptySprite;
    }

    public void FillWith(LiquidData liquid)
    {
        CurrentLiquid = liquid;
        var sprite = filledBucketRepo.GetSprite(liquid);
        spriteRenderer.sprite = sprite;
    }
}