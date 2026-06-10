using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedEmitter : MonoBehaviour
{
    const bool RESET_WHEN_OTHER_EMITTER_TRIGGERS = true;

    [SerializeField] SignalEmitter emitter;
    [SerializeField] SignalType[] signals;
    [SerializeField] float signalInterval;
    [SerializeField] float pauseDuration;
    [SerializeField] Image progressBar;

    private Coroutine timer;
    private int signalIndex;

    void Start()
    {
        StartTimer();
        Room.SignalChanged += OnSignalChanged;
    }

    void OnDestroy()
    {
        Room.SignalChanged -= OnSignalChanged;
    }

    private void OnSignalChanged(SignalType _)
    {
        if (!RESET_WHEN_OTHER_EMITTER_TRIGGERS || progressBar.fillAmount == 1)
            return;
        StartTimer();
    }

    private void StartTimer()
    {
        if (progressBar == null)
            return;

        if (signalInterval <= 0)
            Debug.LogWarning("Signal Interval must be greater than 0. ", this);

        progressBar.fillAmount = 0f;

        if (timer != null)
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