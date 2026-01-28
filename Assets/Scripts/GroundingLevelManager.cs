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
        Debug.Log("<color=green>✓ Vision cleared! You can see the world again.</color>");
        
        // Disable lens distortion completely
        if (lensDistortion != null) lensDistortion.active = false;
        if (chromaticAberration != null) chromaticAberration.active = false;

        // TODO: Trigger next task (Touch - 4 objects)
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
