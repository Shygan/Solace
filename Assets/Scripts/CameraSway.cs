using UnityEngine;

/// <summary>
/// Creates an anxious "sway" effect by gently tilting the camera using sine waves.
/// This represents the unstable feeling of anxiety and will be disabled as grounding progresses.
/// </summary>
public class CameraSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [Tooltip("How much the camera rotates (in degrees)")]
    [Range(0f, 10f)]
    public float swayIntensity = 2f;

    [Tooltip("How fast the camera sways (lower = slower, more unsettling)")]
    [Range(0.1f, 2f)]
    public float swaySpeed = 0.5f;

    [Tooltip("Offset for randomness")]
    private float phaseOffset;

    [Header("State")]
    [Tooltip("Is the sway effect currently active?")]
    public bool isSwaying = true;

    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.localRotation;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f); // Randomize start phase
    }

    void Update()
    {
        if (!isSwaying)
        {
            // Smoothly return to original rotation
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                originalRotation,
                Time.deltaTime * 2f
            );
            return;
        }

        // Calculate sway using sine wave for smooth oscillation
        float time = Time.time * swaySpeed + phaseOffset;
        float rollSway = Mathf.Sin(time) * swayIntensity;
        float pitchSway = Mathf.Cos(time * 0.7f) * swayIntensity * 0.5f; // Slightly different frequency

        // Apply sway rotation
        Quaternion swayRotation = Quaternion.Euler(pitchSway, 0f, rollSway);
        transform.localRotation = originalRotation * swayRotation;
    }

    /// <summary>
    /// Gradually stops the sway effect over time.
    /// </summary>
    public void StopSway(float fadeTime = 2f)
    {
        StartCoroutine(FadeOutSway(fadeTime));
    }

    private System.Collections.IEnumerator FadeOutSway(float duration)
    {
        float elapsed = 0f;
        float startIntensity = swayIntensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            swayIntensity = Mathf.Lerp(startIntensity, 0f, elapsed / duration);
            yield return null;
        }

        swayIntensity = 0f;
        isSwaying = false;
    }
}
