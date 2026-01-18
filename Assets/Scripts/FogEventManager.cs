using UnityEngine;
using System.Collections;

public class FogEventManager : MonoBehaviour
{
    [Header("References")]
    public FogController fogController;
    public HoldToBreathe holdToBreathe;

    [Header("Timing")]
    public float intervalSeconds = 15f; // time between fog events
    public bool startOnPlay = true;

    private Coroutine waitRoutine;

    void Start()
    {
        if (startOnPlay)
            QueueNextTrigger();
    }

    private void OnEnable()
    {
        HoldToBreathe.OnBreathingCycleComplete += OnBreathingCycleComplete;
    }

    private void OnDisable()
    {
        HoldToBreathe.OnBreathingCycleComplete -= OnBreathingCycleComplete;
        StopWaiting();
    }

    private void QueueNextTrigger()
    {
        if (waitRoutine == null)
            waitRoutine = StartCoroutine(WaitThenTrigger());
    }

    private void StopWaiting()
    {
        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }
    }

    private IEnumerator WaitThenTrigger()
    {
        // Wait interval, then trigger once
        yield return new WaitForSeconds(intervalSeconds);
        waitRoutine = null;
        TriggerOnce();
    }

    // Manual trigger for testing from other scripts or UI
    public void TriggerOnce()
    {
        if (holdToBreathe != null && !holdToBreathe.IsBreathingActive())
        {
            if (fogController != null)
                fogController.ShowFog();

            holdToBreathe.StartBreathingCycle();
        }
    }

    private void OnBreathingCycleComplete()
    {
        // Start counting down once the full cycle finishes
        QueueNextTrigger();
    }
}
