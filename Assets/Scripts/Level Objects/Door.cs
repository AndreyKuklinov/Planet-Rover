using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : LevelObject
{
    [SerializeField] Signal signal;
    [SerializeField] Sprite openSprite;
    [SerializeField] Sprite closedSprite;

    private bool isOpen;

    public override bool CanHandGoThrough
        => isOpen;

    protected override void Start()
    {
        Level.SignalsChanged += OnSignalsChanged;
        base.Start();
    }

    protected override void OnDestroy()
    {
        Level.SignalsChanged -= OnSignalsChanged;
        base.OnDestroy();
    }

    private void OnSignalsChanged(HashSet<Signal> obj)
    {
        isOpen = obj.Contains(signal);
        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
    }

    private void OnValidate()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        spriteRenderer.color = SignalColor.GetColor(signal);
    }
}
