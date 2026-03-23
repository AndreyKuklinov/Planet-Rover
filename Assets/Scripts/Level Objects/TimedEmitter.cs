using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedEmitter : LevelObject
{
    [SerializeField] SignalEmitter emitter;
    [SerializeField] Signal[] signals;
    [SerializeField] float signalInterval;
    [SerializeField] float pauseDuration;
    [SerializeField] bool disableWhenOthersEmit;
    [SerializeField] Image progressBar;

    private Coroutine timer;
    private int signalIndex;

    protected override void Start()
    {
        StartTimer();
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
        if (!disableWhenOthersEmit || progressBar.fillAmount == 1)
            return;
        StartTimer();
    }

    private void StartTimer()
    {
        if (progressBar == null)
            return;

        if(signalInterval <= 0)
            Debug.LogWarning("Signal Interval must be greater than 0. ", this);

        progressBar.fillAmount = 0f;

        if(timer != null)
            StopCoroutine(timer);

        timer = StartCoroutine(EmissionCycleRoutine());
    }

    private IEnumerator EmissionCycleRoutine()
    {
        float timer = 0f;

        while (timer < signalInterval)
        {
            timer += Time.deltaTime;
            progressBar.fillAmount = timer / signalInterval;
            yield return null;
        }

        progressBar.fillAmount = 1f;
        emitter.Emit(signals[signalIndex]);
        yield return new WaitForSeconds(pauseDuration);
        ChangeSignal();
        StartTimer();
    }

    private void OnValidate()
    {
        UpdateColor();
    }

    private void UpdateColor()
    {
        if (progressBar == null || signals == null || signals.Length == 0)
            return;

        progressBar.color = SignalColor.GetColor(signals[signalIndex]);
    }

    private void ChangeSignal()
    {
        signalIndex = (signalIndex + 1) % signals.Length;
        UpdateColor();
    }
}