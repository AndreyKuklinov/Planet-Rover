using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimedEmitter : MonoBehaviour
{
    const bool RESET_WHEN_OTHER_EMITTER_TRIGGERS = true;

    [SerializeField] ItemColorDataEventChannel roomSignalChanged;
    [SerializeField] SignalEmitter emitter;
    [SerializeField] List<SignalInterval> signalIntervals;
    [SerializeField] Image progressBar;

    private Coroutine emissionCoroutine;
    private int intervalIndex;

    ItemColorData NextSignal
        => signalIntervals[intervalIndex].Signal;

    float NextIntervalDuration
        => signalIntervals[intervalIndex].TimeBeforeActivation;

    void Start()
    {
        if (signalIntervals == null || signalIntervals.Count == 0)
        {
            Debug.LogWarning("TimedEmitter has no signal intervals assigned!", this);
            return;
        }

        StartTimer();
        if (roomSignalChanged != null)
            roomSignalChanged.Raised += OnSignalChanged;
    }

    void OnDestroy()
    {
        if (roomSignalChanged != null)
            roomSignalChanged.Raised -= OnSignalChanged;
    }

    private void OnSignalChanged(ItemColorData _)
    {
        if (!RESET_WHEN_OTHER_EMITTER_TRIGGERS || (progressBar != null && Mathf.Approximately(progressBar.fillAmount, 1f)))
            return;

        StartTimer();
    }

    private void StartTimer()
    {
        UpdateColor();

        if (progressBar != null)
            progressBar.fillAmount = 0f;

        if (emissionCoroutine != null)
            StopCoroutine(emissionCoroutine);

        emissionCoroutine = StartCoroutine(EmissionCycleRoutine());
    }

    private IEnumerator EmissionCycleRoutine()
    {
        float elapsedTime = 0f;
        float duration = NextIntervalDuration;

        if (duration <= 0f)
        {
            if (progressBar != null) progressBar.fillAmount = 1f;
            yield return null;
        }
        else
        {
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                if (progressBar != null)
                {
                    progressBar.fillAmount = elapsedTime / duration;
                }
                yield return null;
            }
        }

        if (progressBar != null)
            progressBar.fillAmount = 1f;

        if (emitter != null)
            emitter.Emit(NextSignal);

        ChangeSignal();
        StartTimer();
    }

    private void UpdateColor()
    {
        if (progressBar == null || signalIntervals == null || signalIntervals.Count == 0)
            return;

        progressBar.color = NextSignal.Color;
    }

    private void ChangeSignal()
    {
        intervalIndex = (intervalIndex + 1) % signalIntervals.Count;
        UpdateColor();
    }

    [Serializable]
    public struct SignalInterval
    {
        public ItemColorData Signal;
        public float TimeBeforeActivation;
    }
}