using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages the Door of Grounding level progression based on the 5-4-3-2-1 technique.
/// Progressively removes visual distortion as the player completes sensory tasks.
/// </summary>
public class GroundingLevelManager : MonoBehaviour
{
    [Header("Post-Processing References")]
    [Tooltip("Drag the Global Volume GameObject here")]
    public Volume globalVolume;

    /// <summary>
    /// Event fired when a grounding object is found. 
    /// Reuses the same ProgressBar that listens to OnAppleCollect.
    /// </summary>
    public static event Action<int> OnGroundingObjectFound;

    [Header("Environment Grayscale")]
    [Tooltip("Material used for environment sprites to control grayscale")]
    public Material environmentGrayscaleMaterial;

    [Header("Task Tracking")]
    [SerializeField] private int sightObjectsRequired = 5;
    private int sightObjectsFound = 0;
    
    [SerializeField] private int touchObjectsRequired = 4;
    private int touchObjectsFound = 0;
    
    [SerializeField] private int soundObjectsRequired = 3;
    private int soundObjectsFound = 0;
    
    [SerializeField] private int smellObjectsRequired = 2;
    private int smellObjectsFound = 0;
    
    [SerializeField] private int tasteObjectsRequired = 1;
    private int tasteObjectsFound = 0;
    
    private bool sightTaskComplete = false;
    private bool touchTaskComplete = false;
    private bool soundTaskComplete = false;
    private bool smellTaskComplete = false;
    private bool tasteTaskComplete = false;
    
    // Track which task has been explicitly started (not just completed previous)
    private bool touchTaskStarted = false;
    private bool soundTaskStarted = false;
    private bool smellTaskStarted = false;
    private bool tasteTaskStarted = false;

    [Header("Color Hunt Settings")]
    [Tooltip("The color players need to find (will be set by AI)")]
    public Color targetColor = Color.blue;
    
    [Tooltip("How close a color needs to be to match (0-1, lower = stricter)")]
    [Range(0f, 1f)]
    public float colorMatchTolerance = 0.3f;

    // Post-processing effect references
    private LensDistortion lensDistortion;
    private ChromaticAberration chromaticAberration;
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        InitializePostProcessing();
        ApplyInitialDistortion();
        StartSightTask();
    }

    void InitializePostProcessing()
    {
        if (globalVolume == null)
        {
            Debug.LogError("Global Volume not assigned! Please drag the Volume GameObject to GroundingLevelManager.");
            return;
        }

        // Get references to all post-processing effects
        globalVolume.profile.TryGet(out lensDistortion);
        globalVolume.profile.TryGet(out chromaticAberration);
        globalVolume.profile.TryGet(out colorAdjustments);

        if (lensDistortion == null || chromaticAberration == null || colorAdjustments == null)
        {
            Debug.LogError("Missing post-processing effects! Make sure Volume Profile has Lens Distortion, Chromatic Aberration, and Color Adjustments.");
        }
    }

    void ApplyInitialDistortion()
    {
        // Set initial distortion at start (reduced for moving camera comfort)
        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = 0.3f; // Reduced from 1.0 - subtle warping
            lensDistortion.active = true;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0.3f; // Reduced from 0.5
            chromaticAberration.active = true;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = -100f; // Grayscale
            colorAdjustments.active = true;
        }
    }

    void StartSightTask()
    {
        // TODO: Integrate with AI to generate prompt
        // For now, using a default color
        Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(targetColor)}>Find {sightObjectsRequired} {targetColor.ToString()} things to clear your vision.</color>");
    }

    /// <summary>
    /// Called by InteractableGroundingObject when a sight object is found.
    /// </summary>
    public void OnSightObjectFound()
    {
        if (sightTaskComplete) return; // Don't process if task is already complete
        
        sightObjectsFound++;
        Debug.Log($"Sight objects found: {sightObjectsFound}/{sightObjectsRequired}");

        // Fire event for ProgressBar to listen to (same as OnAppleCollect)
        OnGroundingObjectFound?.Invoke(1); // Each object found = 1 unit of progress

        // Progressively reduce distortion
        float progress = (float)sightObjectsFound / sightObjectsRequired;
        
        // Reduce environment grayscale
        if (environmentGrayscaleMaterial != null)
        {
            float grayscaleAmount = Mathf.Lerp(1f, 0f, progress);
            environmentGrayscaleMaterial.SetFloat("_GrayscaleAmount", grayscaleAmount);
        }
        
        if (lensDistortion != null)
        {
            // Smoothly reduce lens distortion from 0.3 to 0
            lensDistortion.intensity.value = Mathf.Lerp(0.3f, 0f, progress);
        }

        if (chromaticAberration != null)
        {
            // Reduce chromatic aberration as well
            chromaticAberration.intensity.value = Mathf.Lerp(0.3f, 0f, progress);
        }

        // Check if task is complete
        if (sightObjectsFound >= sightObjectsRequired)
        {
            CompleteSightTask();
        }
    }

    void CompleteSightTask()
    {
        sightTaskComplete = true;
        Debug.Log("<color=green>✓ Vision cleared! You can see the world again.</color>");
        
        // Disable lens distortion completely
        if (lensDistortion != null) lensDistortion.active = false;
        if (chromaticAberration != null) chromaticAberration.active = false;

        // The DoorOfGroundingController will show the prompt/knob and call StartTouchTask when player holds E
    }
    
    /// <summary>
    /// Called by DoorOfGroundingController after player holds E to proceed.
    /// Starts the touch task (4 objects).
    /// </summary>
    public void StartTouchTask()
    {
        touchTaskStarted = true;
        Debug.Log("<color=cyan>Starting Touch Task: Find 4 things you can touch.</color>");
        touchObjectsFound = 0;
        
        // Enable all touch objects in the scene
        InteractableGroundingObject[] allObjects = FindObjectsOfType<InteractableGroundingObject>();
        foreach (var obj in allObjects)
        {
            if (obj.taskType == GroundingTaskType.Touch)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Called by InteractableGroundingObject when a touch object is found.
    /// </summary>
    public void OnTouchObjectFound()
    {
        if (touchTaskComplete) return; // Don't process if task is already complete
        
        touchObjectsFound++;
        Debug.Log($"Touch objects found: {touchObjectsFound}/{touchObjectsRequired}");

        // Fire event for ProgressBar to listen to
        OnGroundingObjectFound?.Invoke(1);

        // Progressively restore color saturation
        float progress = (float)touchObjectsFound / touchObjectsRequired;
        
        if (colorAdjustments != null)
        {
            // Gradually restore color from grayscale (-100) to full color (0)
            colorAdjustments.saturation.value = Mathf.Lerp(-100f, 0f, progress);
        }

        // Check if task is complete
        if (touchObjectsFound >= touchObjectsRequired)
        {
            CompleteTouchTask();
        }
    }
    
    void CompleteTouchTask()
    {
        touchTaskComplete = true;
        Debug.Log("<color=green>✓ Touch task complete! Colors are returning.</color>");
        
        // Ensure color is fully restored
        if (colorAdjustments != null)
        {
            colorAdjustments.saturation.value = 0f;
        }

        // The DoorOfGroundingController will show prompt/knob and call StartSoundTask when player holds E
    }
    
    /// <summary>
    /// Called by DoorOfGroundingController after player holds E to proceed.
    /// Starts the sound task (3 objects).
    /// </summary>
    public void StartSoundTask()
    {
        soundTaskStarted = true;
        Debug.Log("<color=yellow>Starting Sound Task: Find 3 things you can hear.</color>");
        soundObjectsFound = 0;
        
        // Enable all sound objects in the scene
        InteractableGroundingObject[] allObjects = FindObjectsOfType<InteractableGroundingObject>();
        foreach (var obj in allObjects)
        {
            if (obj.taskType == GroundingTaskType.Sound)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Called by InteractableGroundingObject when a sound object is found.
    /// </summary>
    public void OnSoundObjectFound()
    {
        if (soundTaskComplete) return;
        
        soundObjectsFound++;
        Debug.Log($"Sound objects found: {soundObjectsFound}/{soundObjectsRequired}");

        OnGroundingObjectFound?.Invoke(1);

        // Check if task is complete
        if (soundObjectsFound >= soundObjectsRequired)
        {
            CompleteSoundTask();
        }
    }
    
    void CompleteSoundTask()
    {
        soundTaskComplete = true;
        Debug.Log("<color=green>✓ Sound task complete! You're more aware of your surroundings.</color>");
    }
    
    /// <summary>
    /// Called by DoorOfGroundingController after player holds E to proceed.
    /// Starts the smell task (2 objects).
    /// </summary>
    public void StartSmellTask()
    {
        smellTaskStarted = true;
        Debug.Log("<color=magenta>Starting Smell Task: Find 2 things you can smell.</color>");
        smellObjectsFound = 0;
        
        // Enable all smell objects in the scene
        InteractableGroundingObject[] allObjects = FindObjectsOfType<InteractableGroundingObject>();
        foreach (var obj in allObjects)
        {
            if (obj.taskType == GroundingTaskType.Smell)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Called by InteractableGroundingObject when a smell object is found.
    /// </summary>
    public void OnSmellObjectFound()
    {
        if (smellTaskComplete) return;
        
        smellObjectsFound++;
        Debug.Log($"Smell objects found: {smellObjectsFound}/{smellObjectsRequired}");

        OnGroundingObjectFound?.Invoke(1);

        // Check if task is complete
        if (smellObjectsFound >= smellObjectsRequired)
        {
            CompleteSmellTask();
        }
    }
    
    void CompleteSmellTask()
    {
        smellTaskComplete = true;
        Debug.Log("<color=green>✓ Smell task complete! Your senses are sharpening.</color>");
    }
    
    /// <summary>
    /// Called by DoorOfGroundingController after player holds E to proceed.
    /// Starts the taste task (1 object).
    /// </summary>
    public void StartTasteTask()
    {
        tasteTaskStarted = true;
        Debug.Log("<color=orange>Starting Taste Task: Find 1 thing you can taste.</color>");
        tasteObjectsFound = 0;
        
        // Enable all taste objects in the scene
        InteractableGroundingObject[] allObjects = FindObjectsOfType<InteractableGroundingObject>();
        foreach (var obj in allObjects)
        {
            if (obj.taskType == GroundingTaskType.Taste)
            {
                obj.gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Called by InteractableGroundingObject when a taste object is found.
    /// </summary>
    public void OnTasteObjectFound()
    {
        if (tasteTaskComplete) return;
        
        tasteObjectsFound++;
        Debug.Log($"Taste objects found: {tasteObjectsFound}/{tasteObjectsRequired}");

        OnGroundingObjectFound?.Invoke(1);

        // Check if task is complete
        if (tasteObjectsFound >= tasteObjectsRequired)
        {
            CompleteTasteTask();
        }
    }
    
    void CompleteTasteTask()
    {
        tasteTaskComplete = true;
        Debug.Log("<color=green>✓ Taste task complete! You are fully grounded in the present moment.</color>");
    }
    
    /// <summary>
    /// Check if the touch task is currently active.
    /// </summary>
    public bool IsTouchTaskActive()
    {
        return touchTaskStarted && !touchTaskComplete;
    }
    
    /// <summary>
    /// Check if the sound task is currently active.
    /// </summary>
    public bool IsSoundTaskActive()
    {
        return soundTaskStarted && !soundTaskComplete;
    }
    
    /// <summary>
    /// Check if the smell task is currently active.
    /// </summary>
    public bool IsSmellTaskActive()
    {
        return smellTaskStarted && !smellTaskComplete;
    }
    
    /// <summary>
    /// Check if the taste task is currently active.
    /// </summary>
    public bool IsTasteTaskActive()
    {
        return tasteTaskStarted && !tasteTaskComplete;
    }

    /// <summary>
    /// Check if a sprite's color matches the target color within tolerance.
    /// </summary>
    public bool DoesColorMatch(Color spriteColor)
    {
        // Debug: print the colors being compared
        Debug.Log($"[GroundingLevelManager] Comparing colors - Sprite: RGB({spriteColor.r:F2}, {spriteColor.g:F2}, {spriteColor.b:F2}), Target: RGB({targetColor.r:F2}, {targetColor.g:F2}, {targetColor.b:F2})");
        
        // Calculate color distance using HSV for better perceptual matching
        Color.RGBToHSV(spriteColor, out float h1, out float s1, out float v1);
        Color.RGBToHSV(targetColor, out float h2, out float s2, out float v2);

        Debug.Log($"[GroundingLevelManager] Sprite HSV: ({h1:F2}, {s1:F2}, {v1:F2}), Target HSV: ({h2:F2}, {s2:F2}, {v2:F2})");

        // Hue wraps around (0-1), so handle circular distance
        float hueDiff = Mathf.Abs(h1 - h2);
        if (hueDiff > 0.5f) hueDiff = 1f - hueDiff;

        float satDiff = Mathf.Abs(s1 - s2);
        float valDiff = Mathf.Abs(v1 - v2);

        // Weighted color distance
        float colorDistance = Mathf.Sqrt(
            hueDiff * hueDiff * 2f + 
            satDiff * satDiff + 
            valDiff * valDiff
        );

        Debug.Log($"[GroundingLevelManager] Color distance: {colorDistance:F3}, Tolerance: {colorMatchTolerance:F3}, Match: {colorDistance <= colorMatchTolerance}");

        return colorDistance <= colorMatchTolerance;
    }
}
