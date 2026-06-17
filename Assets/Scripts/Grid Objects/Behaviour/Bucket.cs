using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IHasItemData
{
    [SerializeField] FilledBucketSpriteRepo filledBucketRepo;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ItemData emptyBucketData;
    [SerializeField] Sprite emptySprite;

    public LiquidData CurrentLiquid { get; private set; }

    public bool IsFilled
        => CurrentLiquid != null;

    public ItemData ItemData
        => IsFilled ? CurrentLiquid.ItemData : emptyBucketData;

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