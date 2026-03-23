using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : LevelObject
{
    [SerializeField] Signal signal;
    [SerializeField] Sprite openSprite;
    [SerializeField] Sprite closedSprite;
    [SerializeField] bool isOpen;

    public override bool CanHandGoThrough
        => isOpen;

    public override bool CanReceive(LevelObject levelObject)
    {
        var key = levelObject as DoorKey;
        if (key == null)
            return false;
        return key.Signal == signal;
    }

    public override void Receive(LevelObject levelObject)
    {
        if (!CanReceive(levelObject))
            throw new ArgumentException("Can't drop " + levelObject + " on " + this);

        isOpen = true;
        UpdateSprite();
    }

    protected override void Start()
    {
        Level.SignalChanged += OnSignalChanged;
        base.Start();
    }

    protected override void OnDestroy()
    {
        Level.SignalChanged -= OnSignalChanged;
        base.OnDestroy();
    }

    private void OnSignalChanged(Signal obj)
    {
        isOpen = obj == signal;
        UpdateSprite();
    }

    private void OnValidate()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;

            UpdateColor();
            UpdateSprite();
        };
    #endif
    }

    private void UpdateColor()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = SignalColor.GetColor(signal);
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
    }
}
