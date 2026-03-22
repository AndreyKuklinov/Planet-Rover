using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelButton : LevelObject
{
    public static event Action<Signal> ButtonPressed;

    [SerializeField] SignalEmitter emitter;
    [SerializeField] Signal signal;

    protected override void Start()
    {
        ButtonPressed += OnButtonPressed;
        base.Start();
    }

    protected override void OnDestroy()
    {
        ButtonPressed -= OnButtonPressed;
        base.OnDestroy();
    }

    public override bool CanReceive(LevelObject levelObject)
    {
        return levelObject == null;
    }

    public override void Receive(LevelObject levelObject)
    {
        Debug.Log(signal.ToString() + " button pressed");
        emitter.SetSignals(signal);
        ButtonPressed?.Invoke(signal);
    }

    private void OnButtonPressed(Signal s)
    {
        if (s == signal)
            return;

        emitter.SetSignals();
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
