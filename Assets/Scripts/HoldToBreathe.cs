using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HoldToBreathe : MonoBehaviour
{
    [Header("Breathing Settings")]
    public float phaseDuration = 4f; // 4 seconds per phase
    
    [Header("UI References")]
    public Image fillCircle; // The radial fill circle (reuse from 3-3-3)
    public GameObject inhalePrompt; // "Hold E to Inhale" text
    public GameObject holdPrompt; // "Release E - Hold Breath" text  
    public GameObject exhalePrompt; // "Hold E to Exhale" text
    
    [Header("Fog Reference")]
    public FogController fogController;
    
    public static event Action OnBreathingCycleComplete;
    
    private enum BreathingPhase
    {
        Inhale = 0,          // Hold E for 4 seconds
        HoldAfterInhale = 1, // Release E for 4 seconds
        Exhale = 2,          // Hold E for 4 seconds
        HoldAfterExhale = 3  // Release E for 4 seconds
    }
    
    private BreathingPhase currentPhase = BreathingPhase.Inhale;
    private float phaseTimer = 0;
    private bool isBreathingActive = false;
    private bool isEKeyHeld = false;
    private bool prevEKeyHeld = false;
    private bool hasReachedMidpoint = false; // Track if we've cleared fog at 50%
    
    void Update()
    {
        if (!isBreathingActive)
            return;
        
        // Check E key state
        isEKeyHeld = Input.GetKey(KeyCode.E);
        
        // If user breaks the rule for the phase, reset progress of the current phase
        if (currentPhase == BreathingPhase.Inhale || currentPhase == BreathingPhase.Exhale)
        {
            // Releasing E mid-phase resets progress
            if (prevEKeyHeld && !isEKeyHeld && phaseTimer > 0f)
            {
                ResetCurrentPhaseProgress("Released E during hold phase (inhale/exhale)");
            }
        }
        else // Hold phases: any key press resets progress
        {
            if (Input.anyKeyDown && phaseTimer > 0f)
            {
                ResetCurrentPhaseProgress("Pressed a key during hold phase");
            }
        }

        // Advance timer ONLY when the input matches the required state
        bool shouldHoldKey = currentPhase == BreathingPhase.Inhale || currentPhase == BreathingPhase.Exhale;
        bool inputCorrect = shouldHoldKey ? isEKeyHeld : !isEKeyHeld;
        if (inputCorrect)
        {
            phaseTimer += Time.deltaTime;
        }
        
        // Calculate progress for current phase only (0-1 over 4 seconds)
        float phaseProgress = Mathf.Clamp01(phaseTimer / phaseDuration);
        fillCircle.fillAmount = phaseProgress;
        
        // Update which prompt is visible
        UpdatePromptVisibility();
        
        // Validate input and advance phases
        ValidatePhaseInput();

        // Track previous E state for release detection
        prevEKeyHeld = isEKeyHeld;
    }
    
    private void UpdatePromptVisibility()
    {
        // Hide all prompts first
        if (inhalePrompt != null) inhalePrompt.SetActive(false);
        if (holdPrompt != null) holdPrompt.SetActive(false);
        if (exhalePrompt != null) exhalePrompt.SetActive(false);
        
        // Show the appropriate prompt for current phase
        switch (currentPhase)
        {
            case BreathingPhase.Inhale:
                if (inhalePrompt != null) inhalePrompt.SetActive(true);
                break;
            case BreathingPhase.HoldAfterInhale:
            case BreathingPhase.HoldAfterExhale:
                if (holdPrompt != null) holdPrompt.SetActive(true);
                break;
            case BreathingPhase.Exhale:
                if (exhalePrompt != null) exhalePrompt.SetActive(true);
                break;
        }
    }
    
    private void ValidatePhaseInput()
    {
        // Phase completes strictly when the correct input has filled the timer
        if (phaseTimer >= phaseDuration)
        {
            MoveToNextPhase();
        }
    }
    
    private void MoveToNextPhase()
    {
        int nextPhaseIndex = (int)currentPhase + 1;
        
        // Check if cycle is complete
        if (nextPhaseIndex >= 4)
        {
            CompleteCycle();
            return;
        }
        
        currentPhase = (BreathingPhase)nextPhaseIndex;
        phaseTimer = 0;
        fillCircle.fillAmount = 0;
        
        // After completing Inhale + Hold, we are entering Exhale → clear fog partially once
        if (currentPhase == BreathingPhase.Exhale && !hasReachedMidpoint)
        {
            hasReachedMidpoint = true;
            if (fogController != null)
                fogController.ClearFogPartially();
            Debug.Log("Breathing: Inhale + Hold complete - Fog 50% cleared!");
        }
        
        // Ensure the correct prompt is visible for the new phase
        UpdatePromptVisibility();
        
        Debug.Log($"Breathing: Moving to phase {currentPhase}");
    }

    private void ResetCurrentPhaseProgress(string reason)
    {
        phaseTimer = 0f;
        fillCircle.fillAmount = 0f;
        Debug.Log($"Breathing: Phase progress reset - {reason}");
    }
    
    private void CompleteCycle()
    {
        Debug.Log("Breathing: Full cycle complete! Fog completely cleared.");
        if (fogController != null)
            fogController.ClearFogCompletely();
        OnBreathingCycleComplete?.Invoke();
        EndBreathingCycle();
    }
    
    private void RestartBreathingCycle()
    {
        currentPhase = BreathingPhase.Inhale;
        phaseTimer = 0;
        fillCircle.fillAmount = 0;
        hasReachedMidpoint = false;
        if (fogController != null)
            fogController.ShowFog();
        Debug.Log("Breathing: Cycle restarted from beginning.");
    }
    
    private void EndBreathingCycle()
    {
        isBreathingActive = false;
        currentPhase = BreathingPhase.Inhale;
        phaseTimer = 0;
        fillCircle.fillAmount = 0;
        hasReachedMidpoint = false;
        
        // Hide all UI elements
        if (fillCircle != null)
            fillCircle.gameObject.SetActive(false);
        if (inhalePrompt != null)
            inhalePrompt.SetActive(false);
        if (holdPrompt != null)
            holdPrompt.SetActive(false);
        if (exhalePrompt != null)
            exhalePrompt.SetActive(false);
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
        fillCircle.fillAmount = 0;
        hasReachedMidpoint = false;
        
        // Show fill circle
        if (fillCircle != null)
            fillCircle.gameObject.SetActive(true);
        
        // Update prompts (will show inhale prompt)
        UpdatePromptVisibility();
        
        Debug.Log("Breathing: Cycle started. Hold E to inhale for 4 seconds.");
    }
    
    public bool IsBreathingActive()
    {
        return isBreathingActive;
    }
    
    private int GetCurrentPhaseIndex()
    {
        return (int)currentPhase;
    }
}
