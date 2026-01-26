using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : LevelObject 
{
    [field: SerializeField] public SampleData Data { get; private set; }

    [SerializeField] SpriteRenderer spriteRenderer;

    public override bool CanBeGrabbed
        => true;

    protected override void Start()
    {
        spriteRenderer.sprite = Data.Sprite;
        base.Start();
    }
}
