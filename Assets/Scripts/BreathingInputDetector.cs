using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BreathingInputDetector : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float phaseDuration = 4f; // 4 seconds per phase
    public float timingTolerance = 0.5f; // Allow ±0.5 seconds
    
    [Header("References")]
    public FogController fogController;
    public Image knobUI; // The rotating knob that shows breathing progress
    
    
    public static event Action OnBreathingStarted;
    public static event Action OnPhaseComplete;
    public static event Action OnBreathingCycleComplete;
    
    public enum BreathingPhase
    {
        Inhale = 0,      // Press and hold E
        HoldAfterInhale = 1,  // Release E and hold it released
        Exhale = 2,       // Press and hold E
        HoldAfterExhale = 3   // Release E and hold it released
    }
    
    private BreathingPhase currentPhase = BreathingPhase.Inhale;
    private float phaseTimer = 0;
    private bool isBreathingActive = false;
    private bool isEKeyHeld = false;
    
    void Update()
    {
        if (!isBreathingActive)
            return;
        
        if (fogController == null || knobUI == null)
        {
            Debug.LogError("BreathingInputDetector: FogController or KnobUI not assigned!");
            return;
        }
        
        phaseTimer += Time.deltaTime;
        
        // Update knob rotation based on overall progress (0-360 degrees over full cycle)
        float totalCycleDuration = phaseDuration * 4;
        float cycleProgress = (phaseTimer % totalCycleDuration) / totalCycleDuration;
        knobUI.transform.rotation = Quaternion.Euler(0, 0, -cycleProgress * 360f);
        
        ValidatePhaseInput();
    }
    
    private void ValidatePhaseInput()
    {
        bool phaseComplete = false;
        bool phaseValid = false;
        
        switch (currentPhase)
        {
            case BreathingPhase.Inhale:
                // Player should be holding E
                if (isEKeyHeld && phaseTimer >= phaseDuration - timingTolerance)
                {
                    phaseValid = true;
                    if (phaseTimer >= phaseDuration)
                        phaseComplete = true;
                }
                break;
                
            case BreathingPhase.HoldAfterInhale:
                // Player should have released E
                if (!isEKeyHeld && phaseTimer >= phaseDuration - timingTolerance)
                {
                    phaseValid = true;
                    if (phaseTimer >= phaseDuration)
                    {
                        phaseComplete = true;
                        // Fog clears 50% after first 4-4 (inhale + hold)
                        fogController.ClearFogPartially();
                        Debug.Log("Breathing: Inhale + Hold complete! Fog 50% cleared.");
                    }
                }
                break;
                
            case BreathingPhase.Exhale:
                // Player should be holding E
                if (isEKeyHeld && phaseTimer >= phaseDuration - timingTolerance)
                {
                    phaseValid = true;
                    if (phaseTimer >= phaseDuration)
                        phaseComplete = true;
                }
                break;
                
            case BreathingPhase.HoldAfterExhale:
                // Player should have released E
                if (!isEKeyHeld && phaseTimer >= phaseDuration - timingTolerance)
                {
                    phaseValid = true;
                    if (phaseTimer >= phaseDuration)
                    {
                        phaseComplete = true;
                        // Fog clears completely after full cycle
                        fogController.ClearFogCompletely();
                        Debug.Log("Breathing: Full cycle complete! Fog completely cleared.");
                        OnBreathingCycleComplete?.Invoke();
                        EndBreathingCycle();
                        return;
                    }
                }
                break;
        }
        
        // If player messes up timing, end the cycle
        if (!phaseValid && phaseTimer > phaseDuration + timingTolerance)
        {
            Debug.LogWarning($"Breathing: Incorrect input during {currentPhase} phase. Restarting...");
            RestartBreathingCycle();
            return;
        }
        
        if (phaseComplete)
        {
            OnPhaseComplete?.Invoke();
            MoveToNextPhase();
        }
    }
    
    private void MoveToNextPhase()
    {
        currentPhase = (BreathingPhase)(((int)currentPhase + 1) % 4);
        phaseTimer = 0;
        Debug.Log($"Breathing: Moving to phase {currentPhase}");
    }
    
    private void RestartBreathingCycle()
    {
        currentPhase = BreathingPhase.Inhale;
        phaseTimer = 0;
        isEKeyHeld = false;
        Debug.Log("Breathing: Cycle restarted.");
    }
    
    private void EndBreathingCycle()
    {
        isBreathingActive = false;
        currentPhase = BreathingPhase.Inhale;
        phaseTimer = 0;
        isEKeyHeld = false;
        knobUI.transform.rotation = Quaternion.Euler(0, 0, 0);
        Debug.Log("Breathing: Cycle ended.");
    }
    
    /// <summary>
    /// Call this to start a new breathing cycle
    /// </summary>
    public void StartBreathingCycle()
    {
        if (isBreathingActive)
            return;
        
        isBreathingActive = true;
        currentPhase = BreathingPhase.Inhale;
        phaseTimer = 0;
        isEKeyHeld = false;
        OnBreathingStarted?.Invoke();
        Debug.Log("Breathing: Cycle started. Player must inhale by holding E.");
    }
    
    public bool IsBreathingActive()
    {
        return isBreathingActive;
    }
    
    /// <summary>
    /// Input callback - called when E is pressed
    /// </summary>
    public void OnEKeyPressed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isEKeyHeld = true;
            Debug.Log($"E pressed during {currentPhase}");
        }
        else if (context.canceled)
        {
            isEKeyHeld = false;
            Debug.Log($"E released during {currentPhase}");
        }
    }
    
    public float GetPhaseProgress()
    {
        return Mathf.Clamp01(phaseTimer / phaseDuration);
    }
    
    public BreathingPhase GetCurrentPhase()
    {
        return currentPhase;
    }
}
