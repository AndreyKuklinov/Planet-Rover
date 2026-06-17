using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bucket : MonoBehaviour, IHasItemData, IRecolorable
{
    [SerializeField] FilledBucketSpriteRepo filledBucketRepo;
    [SerializeField] ItemDataRepo itemDataRepo;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] ItemData emptyBucketData;
    [SerializeField] Sprite emptySprite;

    public ItemData CurrentLiquid { get; private set; }

    public bool IsFilled
        => CurrentLiquid != null;

    public ItemData ItemData
        => IsFilled ? CurrentLiquid : emptyBucketData;

    public ItemColorData CurrentColor =>
        ItemData.ColorData;

    public bool CanBeRecolored(ItemColorData _)
    {
        return IsFilled;
    }

    public void Empty()
    {
        CurrentLiquid = null;
        spriteRenderer.sprite = emptySprite;
    }

    public void FillWith(ItemData liquid)
    {
        SetCurrentLiquid(liquid);
    }

    public void Recolor(ItemColorData newColor)
    {
        if (!CanBeRecolored(newColor))
            throw new InvalidOperationException("Trying to recolor a bucket that is not filled");

        var liquidType = CurrentLiquid.TypeData;
        var newLiquid = itemDataRepo.GetItemData(liquidType, newColor);
        SetCurrentLiquid(newLiquid);
    }

    private void SetCurrentLiquid(ItemData liquid)
    {
        CurrentLiquid = liquid;
        var sprite = filledBucketRepo.GetSprite(liquid);
        spriteRenderer.sprite = sprite;
    }
}