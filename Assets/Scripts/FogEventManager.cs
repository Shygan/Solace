using UnityEngine;
using System.Collections;

public class FogEventManager : MonoBehaviour
{
    [Header("References")]
    public FogController fogController;
    public HoldToBreathe holdToBreathe;

    [Header("Timing")]
    public float intervalSeconds = 10f; // time between fog events
    public bool startOnPlay = true;

    private Coroutine loop;

    void Start()
    {
        if (startOnPlay)
            StartLoop();
    }

    public void StartLoop()
    {
        if (loop == null)
            loop = StartCoroutine(FogLoop());
    }

    public void StopLoop()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    private IEnumerator FogLoop()
    {
        // Small initial delay to avoid immediate trigger
        yield return new WaitForSeconds(intervalSeconds);

        while (true)
        {
            // Only trigger when breathing is not already active
            if (holdToBreathe != null && !holdToBreathe.IsBreathingActive())
            {
                if (fogController != null)
                    fogController.ShowFog();

                holdToBreathe.StartBreathingCycle();
            }

            // Wait for next interval
            yield return new WaitForSeconds(intervalSeconds);
        }
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
}
